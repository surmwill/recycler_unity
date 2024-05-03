using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Transform = UnityEngine.Transform;

/// <summary>
/// A scroll rect containing a list of data, only processing that data which can be seen on-screen.
/// 
/// Note: all entries and the end-cap can have auto-calculated dimensions but their widths are by default force expanded to
/// to the width of the viewport. Additionally to prevent spam layout recalculations, once calculated, all layout components
/// (ILayoutElement, ILayoutController) such as VerticalLayoutGroups get disabled; however, this also includes such things as Images.
/// In this case the Image should be moved as a child. 
/// </summary>
public abstract partial class RecyclerScrollRect<TEntryData> : ScrollRect
{
    [Header("Recycler Fields")]
    [SerializeField]
    private RecyclerScrollRectEntry<TEntryData> _recyclerEntryPrefab = null;

    [SerializeField]
    private int _numCachedBeforeStart = 2;
    
    [SerializeField]
    private int _numCachedAfterEnd = 2;

    [SerializeField]
    private bool _isTopDown = true;

    [SerializeField]
    private RectTransform _poolParent = null;
    
    [SerializeField]
    private int _poolSize = 15;

    [SerializeField]
    private RectTransform _endcapParent = null;

    [SerializeField]
    private RecyclerEndcap<TEntryData> _endcapPrefab = null;

    [Tooltip("The direction a growing entry should push itself and consequently the entries above/below it")]
    [SerializeField]
    private bool _onSizeRecalculationGrowShrinkUpwards = false;

    // "Active" = both visible, and non-visible but cached entries 
    private Dictionary<int, RecyclerScrollRectEntry<TEntryData>> _activeEntries = new();

    //private Dictionary<int, RecyclerScrollRectEntry<TEntryData>> _cachedStartEntries = new();
    //private Dictionary<int, RecyclerScrollRectEntry<TEntryData>> _cachedEndEntries = new();

    private SlidingWindow<TEntryData> _slidingWindow;

    private Dictionary<int, RecyclerScrollRectEntry<TEntryData>> _possibleEntriesToRecycleThisFrame = new();
    private Dictionary<int, Queue<RecyclerScrollRectEntry<TEntryData>>> _recycledEntries = new();

    private readonly NegativeGrowingList<TEntryData> _entryData = new();
    private readonly List<TEntryData> _pendingAppendEntryData = new();
    private readonly List<TEntryData> _pendingPrependEntryData = new();
    
    //private int _shownStartIndex;
    //private int _shownEndIndex;
    
    //private int? _lastShownStartIndex;
    //private int? _lastShownEndIndex;
    
    private Vector2 _onStartPivot;
    private RecyclerEndcap<TEntryData> _endcap;

    protected override void Awake()
    {
        base.Awake();
        
        // The base ScrollRect has [ExecuteAlways] but the recycler does not work as such
        if (!Application.isPlaying)
        {
            return;
        }

        _slidingWindow = new SlidingWindow<TEntryData>(_numCachedBeforeStart);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        // The base ScrollRect has [ExecuteAlways] but the recycler does not work as such
        if (!Application.isPlaying)
        {
            return;
        }
        
        AddPendingEntries();
    }

    protected override void Start()
    {
        base.Start();
        
        // The base ScrollRect has [ExecuteAlways] but the recycler does not work as such
        if (!Application.isPlaying)
        {
            return;
        }

        _onStartPivot = content.pivot;
        InitPools();
        AddPendingEntries();
    }

    /// <summary>
    /// Inserts an element at the given index. Note that this implies indices can shift
    /// </summary>
    public void Insert(int index, TEntryData entryData, bool? growUpwards = null)
    {
        // Inserting at the end
        if (index == _entryData.Count || index == _entryData.BackwardCount - 1)
        {
            RecycleEndcap();
        }

        // Shift all existing indices. Note that no transforms are actually moved
        _entryData.Insert(index, entryData);
        if (_entryData.Count > 1)
        {
            ShiftEntries(index, index >= 0, index >= 0 ? 1 : -1);   
        }

        // Entry will get created when we scroll to it
        if (!_slidingWindow.Contains(index))
        {
            return;
        }

        // Find the proper place in the hierarchy for the entry
        int siblingIndex = 0;
        foreach (Transform entryTransform in content)
        {
            RecyclerScrollRectEntry<TEntryData> entry = entryTransform.GetComponent<RecyclerScrollRectEntry<TEntryData>>();
            if (entry == null)
            {
               continue;
            }
            
            // Top-down
            if (_isTopDown)
            {
                if (entry.Index == index - 1)
                {
                    siblingIndex = entryTransform.GetSiblingIndex() + 1;   
                }
                else if (entry.Index == index + 1)
                {
                    siblingIndex = entryTransform.GetSiblingIndex();
                }
            }
            // Bottom-up
            else
            {
                if (entry.Index == index - 1)
                {
                    siblingIndex = entryTransform.GetSiblingIndex();
                }
                else if (entry.Index == index + 1)
                {
                    siblingIndex = entryTransform.GetSiblingIndex() + 1;   
                }
            }
        }

        // Create the entry
        CreateAndAddEntry(index, siblingIndex, isInStartCache ? _isTopDown : (isInEndCache ? !_isTopDown : growUpwards));
        UpdateVisibility();
        SetCacheDirty();
    }

    /// <summary>
    /// Removes an element at the given index. Note that this implies indices can shift
    /// </summary>
    public void RemoveAt(int index, bool? shrinkUpwards = null)
    {
        // Recycle the entry if it exists in the scene
        bool shouldRecycle = _activeEntries.TryGetValue(index, out RecyclerScrollRectEntry<TEntryData> activeEntry);
        if (shouldRecycle)
        {
            SendToRecycling(activeEntry, shrinkUpwards);
        }
        
        // Stop bookkeeping the entry
        _activeEntries.Remove(index);
        _cachedStartEntries.Remove(index);
        _cachedEndEntries.Remove(index);
        _possibleEntriesToRecycleThisFrame.Remove(index);

        // Unbind any entries that are waiting in recycling
        if (!_recycledEntries.TryGetValue(RecyclerScrollRectEntry<TEntryData>.UnboundIndex, out Queue<RecyclerScrollRectEntry<TEntryData>> unboundEntries))
        {
            unboundEntries = new Queue<RecyclerScrollRectEntry<TEntryData>>();
        }
        
        if (_recycledEntries.TryGetValue(index, out Queue<RecyclerScrollRectEntry<TEntryData>> entries))
        {
            foreach (RecyclerScrollRectEntry<TEntryData> entry in entries)
            {
                entry.ResetIndex();
                unboundEntries.Enqueue(entry);
            }

            _recycledEntries.Remove(index);
        }

        _recycledEntries[RecyclerScrollRectEntry<TEntryData>.UnboundIndex] = unboundEntries;
        
        // Shift all existing indices. Note that no transforms are actually moved
        _entryData.RemoveAt(index);
        ShiftEntries(index + (index >= 0 ? 1 : -1), index >= 0, index >= 0 ? -1 : 1);
        
        // Update the visibility if we removed something
        if (shouldRecycle)
        {
            UpdateVisibility();  
            SetCacheDirty();
        }
    }

    private void ShiftEntries(int startAtIndex, bool isDirectionPositive, int shiftAmount)
    {
        // Shift visible window
        (_shownStartIndex, _shownEndIndex) = (GetShiftedIndex(_shownStartIndex), GetShiftedIndex(_shownEndIndex));
        (_lastShownStartIndex, _lastShownEndIndex) = (
            _lastShownStartIndex.HasValue ? GetShiftedIndex(_lastShownStartIndex.Value) : null,
            _lastShownEndIndex.HasValue ? GetShiftedIndex(_lastShownEndIndex.Value) : null);

        // Shift bookkeeping
        Shift(ref _activeEntries);
        Shift(ref _cachedStartEntries);
        Shift(ref _cachedEndEntries);
        Shift(ref _possibleEntriesToRecycleThisFrame);
        
        // Shift bookkeeping of a slightly different type
        Dictionary<int, Queue<RecyclerScrollRectEntry<TEntryData>>> shiftedRecycledEntries = new Dictionary<int, Queue<RecyclerScrollRectEntry<TEntryData>>>();
        foreach ((int index, Queue<RecyclerScrollRectEntry<TEntryData>> entries) in _recycledEntries)
        {
            int shiftedIndex = GetShiftedIndex(index);
            foreach (RecyclerScrollRectEntry<TEntryData> entry in entries)
            {
                entry.SetIndex(shiftedIndex);
            }
            shiftedRecycledEntries[shiftedIndex] = entries;
        }
        _recycledEntries = shiftedRecycledEntries;

        // Helper functions
        void Shift(ref Dictionary<int, RecyclerScrollRectEntry<TEntryData>> entries)
        {
            Dictionary<int, RecyclerScrollRectEntry<TEntryData>> shiftedEntries = new Dictionary<int, RecyclerScrollRectEntry<TEntryData>>();
            foreach ((int index, RecyclerScrollRectEntry<TEntryData> entry) in entries)
            {
                int shiftedIndex = GetShiftedIndex(index);
                entry.SetIndex(shiftedIndex);
                shiftedEntries[shiftedIndex] = entry;
            }
            entries = shiftedEntries;
        }
        
        int GetShiftedIndex(int index)
        {
            bool isIndexInDirection = isDirectionPositive && index >= startAtIndex || !isDirectionPositive && index <= startAtIndex;

            int shiftedIndex = index;
            if (isIndexInDirection && (index >= 0 && startAtIndex >= 0 || index < 0 && startAtIndex < 0) && index != RecyclerScrollRectEntry<TEntryData>.UnboundIndex)
            {
                shiftedIndex = index + shiftAmount;
            }

            return shiftedIndex;
        }
    }

    private void AddPendingEntries()
    {
        if (_pendingPrependEntryData.Any())
        {
            PrependEntries(_pendingPrependEntryData);
            _pendingPrependEntryData.Clear();   
        }

        if (_pendingAppendEntryData.Any())
        {
            AppendEntries(_pendingAppendEntryData);
            _pendingAppendEntryData.Clear();   
        }
    }

    /// <summary>
    /// Adds entries to the end of the list
    /// </summary>
    public void AppendEntries(IEnumerable<TEntryData> entries)
    {
        RecycleEndcap();
        if (gameObject.activeInHierarchy)
        {
            AddEntries(entries, true);
            return;
        }
        _pendingAppendEntryData.AddRange(entries);
    }

    /// <summary>
    /// Adds entries to the start of the list
    /// </summary>
    public void PrependEntries(IEnumerable<TEntryData> entries)
    {
        if (gameObject.activeInHierarchy)
        {
            AddEntries(entries, false);
            return;
        }
        _pendingPrependEntryData.AddRange(entries);
    }

    /// <summary>
    /// Adds additional entries to display
    /// </summary>
    private void AddEntries(IEnumerable<TEntryData> newEntries, bool shouldAppend)
    {
        if (!newEntries.Any())
        {
            return;
        }

        // Entries request more entries to be created; however, we start out with no entries and therefore need to kick
        // off the creation cycle by creating one ourselves. Insertion causes an immediate creation and so we use that.
        bool areInitialEntries = _entryData.Count == 0;
        if (areInitialEntries)
        {
            TEntryData first = newEntries.First();
            Insert(0, first);
            newEntries = newEntries.Skip(1);
        }

        if (shouldAppend)
        {
            _entryData.Append(newEntries);
        }
        else
        {
            _entryData.Prepend(newEntries);
        }
        
        SetCacheDirty();
    }

    private void SetCacheDirty()
    {
        (_lastShownStartIndex, _lastShownStartIndex) = (null, null);
    }

    /// <summary>
    /// Background: apart from filling up the screen with entries we also append a couple (user defined amount) entries
    /// before and after the first and last entries on screen, effectively creating a bigger list than the view.
    /// This ensures we have something to scroll into instead of creating that something on the fly.
    ///
    /// This updates those appended entries before the first entry that appears on screen. Returns the indices of any new entries
    /// that need to be created and updates our list of entries to recycle
    /// </summary>
    private List<int> UpdateStartCache()
    {
        // Check out-of-bounds
        foreach (RecyclerScrollRectEntry<TEntryData> outOfLimitEntry in _cachedStartEntries
                     .RemoveWhere(kvp => kvp.Value.Index >= _shownStartIndex || kvp.Value.Index < _shownStartIndex - _numCachedBeforeStart)
                     .Select(kvp => kvp.Value))
        {
            _possibleEntriesToRecycleThisFrame[outOfLimitEntry.Index] = outOfLimitEntry;
        }
        
        List<int> createNewEntries = null;
        for (int i = _shownStartIndex - 1; i >= _shownStartIndex - _numCachedBeforeStart && i >= -_entryData.BackwardCount; i--)
        {
            // If we were originally going to recycle it but now we need it in the cache, don't recycle it
            if (_possibleEntriesToRecycleThisFrame.TryGetValue(i, out RecyclerScrollRectEntry<TEntryData> entry))
            {
                _cachedStartEntries[entry.Index] = entry;
                _possibleEntriesToRecycleThisFrame.Remove(entry.Index);
            }
            
            // If it's not already waiting in the cache then create it
            if (!_cachedStartEntries.ContainsKey(i))
            {
                (createNewEntries ??= new List<int>()).Add(i);
            }
        }

        return createNewEntries;
    }

    /// <summary>
    /// Background: apart from filling up the screen with entries we also append a couple (user defined amount) entries
    /// before and after the first and last entries on screen, effectively creating a bigger list than the view.
    /// This ensures we have something to scroll into instead of creating that something on the fly.
    ///
    /// This updates those appended entries after the last entry that appears on screen. Returns the indices of any new entries
    /// that need to be created and updates our list of entries to recycle
    /// </summary>
    private List<int> UpdateEndCache()
    {
        // Check out-of-bounds
        foreach (RecyclerScrollRectEntry<TEntryData> outOfLimitEntry in _cachedEndEntries
                     .RemoveWhere(kvp => kvp.Value.Index <= _shownEndIndex || kvp.Value.Index > _shownEndIndex + _numCachedAfterEnd)
                     .Select(kvp => kvp.Value))
        {
            _possibleEntriesToRecycleThisFrame[outOfLimitEntry.Index] = outOfLimitEntry;
        }

        List<int> createNewEntries = null;
        for (int i = _shownEndIndex + 1; i <= _shownEndIndex + _numCachedAfterEnd && i < _entryData.ForwardCount; i++)
        {
            // If we were originally going to recycle it but now we need it in the cache, don't recycle it
            if (_possibleEntriesToRecycleThisFrame.TryGetValue(i, out RecyclerScrollRectEntry<TEntryData> entry))
            {
                _cachedEndEntries[entry.Index] = entry;
                _possibleEntriesToRecycleThisFrame.Remove(entry.Index);
            }
         
            // If it's not already waiting in the cache then create it
            if (!_cachedEndEntries.ContainsKey(i))
            {
                (createNewEntries ??= new List<int>()).Add(i);
            }
        }

        return createNewEntries;
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        
        // The base ScrollRect has [ExecuteAlways] but the recycler does not work as such
        if (!Application.isPlaying)
        {
            return;
        }

        // Determine what is shown
        UpdateVisibility();
        
        // If the window of shown entries changes we'll need to update the cache accordingly
        while (_lastShownStartIndex != _shownStartIndex || _lastShownEndIndex != _shownEndIndex)
        {
            (_lastShownStartIndex, _lastShownEndIndex) = (_shownStartIndex, _shownEndIndex);

            // Determine what entries belong and don't belong in the cache
            (List<int> newCachedStartIndices, List<int> newCachedEndIndices) = (UpdateStartCache(), UpdateEndCache());
            
            #if UNITY_EDITOR
            DebugPrint();
            #endif

            // Recycle any entries that aren't visible and don't belong in the cache
            foreach ((int _, RecyclerScrollRectEntry<TEntryData> entry) in _possibleEntriesToRecycleThisFrame)
            {
                SendToRecycling(entry);
            }
            _possibleEntriesToRecycleThisFrame.Clear();

            // Create any new cached entries
            foreach (int entryIndex in newCachedStartIndices ?? Enumerable.Empty<int>())
            {
                CreateAndAddEntry(entryIndex, _isTopDown ? 0 : content.transform.childCount, _isTopDown);
            }
            
            foreach (int entryIndex in newCachedEndIndices ?? Enumerable.Empty<int>())
            {
                CreateAndAddEntry(entryIndex, _isTopDown ? content.transform.childCount : 0, !_isTopDown);
            }
            
            // We just added/removed entries. Update the visibility of the new entries and see if we need to do it again
            UpdateVisibility();

            #if UNITY_EDITOR
            void DebugPrint()
            {
                // Comment this to print
                return;
                
                Debug.Log($"Visible indices ({_shownStartIndex}, {_shownEndIndex})");
                
                if (newCachedStartIndices != null)
                {
                    Debug.Log("New cached start entries " + string.Join(",", newCachedStartIndices));   
                }

                if (newCachedEndIndices != null)
                {
                    Debug.Log("New cached end entries " + string.Join(",", newCachedEndIndices));  
                }
            }
            #endif
        }
        
        // Our window of visible entries are up to date. We can check if the end-cap fits now,
        UpdateEndcap();

        // Sanity checks
        if (Application.isEditor)
        {
            DebugCheckDuplicates();  
            DebugCheckOrdering();
        }
    }

    /// <summary>
    /// The end-cap should exist only if the last entry exists
    /// </summary>
    private void UpdateEndcap()
    {
        if (_endcap == null)
        {
            return;
        }
        
        int maxPossibleIndex = _entryData.ForwardCount - 1;
        bool maxEntryExists = _cachedEndEntries.ContainsKey(maxPossibleIndex) ||
                              _cachedStartEntries.ContainsKey(maxPossibleIndex) ||
                              maxPossibleIndex >= _shownStartIndex && maxPossibleIndex <= _shownEndIndex;
        
        // End-cap used to exist but we scrolled away from the end, get rid of it
        if (!maxEntryExists)
        {
            RecycleEndcap();
            return;
        }

        // The last entry exists, ensure we have an end-cap
        if (_endcap.gameObject.activeSelf)
        {
            return;
        }
        
        _endcap.transform.SetParent(content);
        _endcap.gameObject.SetActive(true);
        _endcap.OnBind();
        
        AddToContent(_endcap.RectTransform, _isTopDown ? content.transform.childCount : 0, !_isTopDown);
    }

    private void RecycleEndcap()
    {
        if (_endcap.gameObject.activeSelf)
        {
            RemoveFromContent(_endcap.RectTransform, !_isTopDown).SetParent(_endcapParent);
            _endcap.OnSentToRecycling();
        }
    }

    private RecyclerScrollRectEntry<TEntryData> CreateAndAddEntry(int dataIndex, int siblingIndex, bool? growUpwards = null)
    {
        if (!TryFetchFromRecycling(dataIndex, out RecyclerScrollRectEntry<TEntryData> entry))
        {
            entry = Instantiate(_recyclerEntryPrefab, content);
        }
        
        if (entry.Index != dataIndex)
        {
            entry.BindNewData(dataIndex, _entryData[dataIndex]);
        }
        else
        {
            entry.RebindExistingData();
        }
        
        AddToContent(entry.RectTransform, siblingIndex, growUpwards);
        _activeEntries[dataIndex] = entry;
        
        return entry;
    }
    
    /// <summary>
    /// Updates the window of entries that are shown (which also affects what entries need to be waiting in the cache)
    /// </summary>
    private void UpdateVisibility()
    {
        foreach (RecyclerScrollRectEntry<TEntryData> entry in _activeEntries.Values)
        {
            bool isVisible = entry.RectTransform.Overlaps(viewport);
            if (isVisible)
            {
                EntryIsVisible(entry);
            }
            else
            {
                EntryIsNotVisible(entry);
            }
        }

        // Visible
        void EntryIsVisible(RecyclerScrollRectEntry<TEntryData> entry)
        {
            _possibleEntriesToRecycleThisFrame.Remove(entry.Index);
            _cachedStartEntries.Remove(entry.Index);
            _cachedEndEntries.Remove(entry.Index);

            int entryIndex = entry.Index;
            if (entryIndex < _shownStartIndex)
            {
                _shownStartIndex = entryIndex;
            }
            else if (entryIndex > _shownEndIndex)
            {
                _shownEndIndex = entryIndex;
            }
        }

        // Not visible
        void EntryIsNotVisible(RecyclerScrollRectEntry<TEntryData> entry)
        {
            int entryIndex = entry.Index;
            bool wentOffTop = entry.RectTransform.position.y > viewport.transform.position.y;
            _possibleEntriesToRecycleThisFrame[entryIndex] = entry;

            if (_isTopDown)
            {
                if (wentOffTop && _shownStartIndex <= entryIndex)
                {
                    _shownStartIndex = entryIndex + 1;
                }
                else if (!wentOffTop && _shownEndIndex >= entryIndex)
                {
                    _shownEndIndex = entryIndex - 1;
                }
            }
            else
            {
                if (wentOffTop && _shownEndIndex >= entryIndex)
                {
                    _shownEndIndex = entryIndex - 1;
                }
                else if (!wentOffTop && _shownStartIndex <= entryIndex)
                {
                    _shownStartIndex = entryIndex + 1;
                }
            }
        }
    }

    /// <summary>
    /// Shows the beginning of the list
    /// </summary>
    public void ResetToBeginning()
    {
        List<TEntryData> entryData = _entryData.ToList();
        Clear();
        AppendEntries(entryData);
    }
    
    /// <summary>
    /// Resets the scroll rect to its initial state with no entries
    /// </summary>
    public void Clear()
    {
        // Stop any active dragging
        OnEndDrag(new PointerEventData(EventSystem.current));
        StopMovement();
        
        // Recycle everything
        for (int i = content.transform.childCount - 1; i >= 0; i--)
        {
            RecyclerScrollRectEntry<TEntryData> entry = content.GetChild(i).GetComponent<RecyclerScrollRectEntry<TEntryData>>();
            if (entry != null)
            {
                SendToRecycling(entry);
            }
        }
        
        // Recycle the end-cap if it exists
        RecycleEndcap();
        
        // Clear state
        _activeEntries.Clear();
        
        _cachedStartEntries.Clear();
        _cachedEndEntries.Clear();
        
        _recycledEntries.Clear();
        _possibleEntriesToRecycleThisFrame.Clear();
        
        _entryData.Clear();
        _pendingAppendEntryData.Clear();
        _pendingPrependEntryData.Clear();
        
        (_shownStartIndex, _shownEndIndex) = (0, 0);
        (_lastShownStartIndex, _lastShownEndIndex) = (null, null);

        // Reset our pivot to whatever its initial value was
        content.pivot = _onStartPivot;
        normalizedPosition = normalizedPosition.WithY(0f);
        
        // Reset the pools
        InitPools();
    }

    private void SendToRecycling(RecyclerScrollRectEntry<TEntryData> entry, bool? shrinkUpwards = null)
    {
        RectTransform entryTransform = entry.RectTransform;
        RemoveFromContent(entryTransform, shrinkUpwards);
        entryTransform.SetParent(_poolParent);

        // Mark the entry as ready for re-use
        if (!_recycledEntries.TryGetValue(entry.Index, out Queue<RecyclerScrollRectEntry<TEntryData>> recycled))
        {
            recycled = new Queue<RecyclerScrollRectEntry<TEntryData>>();
            _recycledEntries.Add(entry.Index, recycled);
        }
        recycled.Enqueue(entry);

        _activeEntries.Remove(entry.Index);
        entry.OnRecycled();
    }

    private bool TryFetchFromRecycling(int entryIndex, out RecyclerScrollRectEntry<TEntryData> entry)
    {
        if (!_recycledEntries.Any())
        {
            entry = null;
            return false;
        }

        int recycledEntryIndex = default;
        Queue<RecyclerScrollRectEntry<TEntryData>> recycledEntries = null;

        if (_recycledEntries.TryGetValue(entryIndex, out recycledEntries))
        {
            recycledEntryIndex = entryIndex;
        }
        else if (_recycledEntries.TryGetValue(RecyclerScrollRectEntry<TEntryData>.UnboundIndex, out recycledEntries))
        {
            recycledEntryIndex = RecyclerScrollRectEntry<TEntryData>.UnboundIndex;
        }
        else
        {
            KeyValuePair<int, Queue<RecyclerScrollRectEntry<TEntryData>>> first = _recycledEntries.First();
            (recycledEntryIndex, recycledEntries) = (first.Key, first.Value);
        }

        entry = recycledEntries.Dequeue();
        if (!recycledEntries.Any())
        {
            _recycledEntries.Remove(recycledEntryIndex);
        }
        
        entry.transform.SetParent(content);
        entry.gameObject.SetActive(true);
        return true;
    }

    /// <summary>
    /// Adds a child under the (parent) content.
    /// 
    /// Children control their own width and height (one exception below). This stems from it being necessary for (the parent) content's layout
    /// group to not "control child size width/height". It is possible for the parent to do so, but every time its size changes
    /// as a result of binding and recycling entries it will force a size re-calculation on each child, possibly getting very expensive.
    /// Therefore children will calculate their own size once, disable their LayoutBehaviours to avoid future size re-calculations,
    /// and relay their size to the parent for it to figure out how it fits in its own layout calculation.
    ///
    /// Exception: the child is force expanded to the viewport width. If the child has auto-calculated height these things are often defined
    /// relative to the viewport width (ex: how does this paragraph of text fit on-screen). Since the parent layout cannot "control child size"
    /// we can't have the parent force expand the width for us; this is our equivalent. Without this we would lose such information.
    /// </summary>
    private void AddToContent(RectTransform child, int siblingIndex, bool? growUpwards = null)
    {
        Behaviour[] layoutBehaviours = LayoutUtilities.GetLayoutBehaviours(child.gameObject, true);
        growUpwards ??= _onSizeRecalculationGrowShrinkUpwards;

        // Proper hierarchy
        child.SetParent(content);
        child.SetSiblingIndex(siblingIndex);
        
        // Force expand the width
        (child.anchorMin, child.anchorMax) = (Vector2.one * 0.5f, Vector2.one * 0.5f);
        child.sizeDelta = child.sizeDelta.WithX(viewport.rect.width);
        
        // Auto-calculate height given the width, then disable layout behaviours to prevent spam recalculations
        SetBehavioursEnabled(layoutBehaviours, true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(child);
        SetBehavioursEnabled(layoutBehaviours, false);
        
        // Now calculate the change in parent size given the child's size
        RecalculateContentSize(growUpwards.Value);
    }

    private RectTransform RemoveFromContent(RectTransform child, bool? shrinkUpwards = null)
    {
        shrinkUpwards ??= _onSizeRecalculationGrowShrinkUpwards;

        // If the child is not visible then shrink in the direction which keeps it off screen and preserves the currently visible entries
        if (!child.Overlaps(viewport))
        {
            shrinkUpwards = child.GetWorldRect().Center.y > viewport.GetWorldRect().Center.y;
        }
        
        // Remove the child and recalculate the parent's size
        child.gameObject.SetActive(false);
        RecalculateContentSize(shrinkUpwards.Value);

        return child;
    }

    /// <summary>
    /// Called when a child needs its dimensions updated
    /// </summary>
    public void RecalculateContentChildSize(RectTransform contentChild, bool? growUpwards = null)
    {
        Assert.IsTrue(contentChild.transform.parent == content);
        growUpwards ??= _onSizeRecalculationGrowShrinkUpwards;

        // If the child is not visible then grow in the direction which keeps it off screen and preserves the currently visible entries
        if (!contentChild.Overlaps(viewport))
        {
            growUpwards = contentChild.GetWorldRect().Center.y > viewport.GetWorldRect().Center.y;
        }

        // Children control their own height (see AddToContent)
        Behaviour[] layoutBehaviours = LayoutUtilities.GetLayoutBehaviours(contentChild.gameObject, true);
        SetBehavioursEnabled(layoutBehaviours, true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentChild);
        SetBehavioursEnabled(layoutBehaviours, false);
        
        // Now calculate the change in parent size given the child's size
        RecalculateContentSize(growUpwards.Value);
    }
    
     /// <summary>
    /// Recalculates the size of the entire ScrollRect. Each child is required to have calculated its size prior to this
    /// (i.e the ScrollRect is not a layout controller).
    /// 
    /// A dynamically sized ScrollRect has 2 problems:
    ///
    /// 1.) Inserting/removing an element will offset and push around the other elements causing things to jump around on-screen. By setting the anchor.y to 1 or 0
    /// we can control this offsetting behaviour. An anchor.y of 0 is like driving a stake into the bottom of the ScrollRect with any size changes coming off the top.
    /// If for example we are adding an element to the top, and all size changes are coming off the top, then only this new element will get pushed around which is fine
    /// as it's being added (not visible) yet. Thus we are able to add/remove elements while keeping a static view of what's currently visible.
    ///
    /// 2.) Inserting/removing an element will cause any held drags to jump. ScrollRects calculate their scroll based on previous and current anchored positions;
    /// if the content size changes then the previous anchored position will be defined relative to a differently sized ScrollRect. We'll get an unnatural jump in values.
    /// Upon resizing we then ensure our anchored position remains the same by moving the anchor itself. 
    /// </summary>
    private void RecalculateContentSize(bool growShrinkUpwards)
    {
        // Initial state
        bool initIsScrollable = this.IsScrollable();
        Vector2 initPivot = content.pivot;
        float initY = content.anchoredPosition.y;
            
        // Special case: (example) if we receive a new text message and we are at the very bottom of the conversation then we should
        // automatically scroll down and show the new message. We should not maintain our view in cases like these 
        bool shouldMaintainTopmost = _isTopDown && growShrinkUpwards && (Mathf.Approximately(normalizedPosition.y, 1f) || normalizedPosition.y > 1f);
        bool shouldMaintainBotmost = !_isTopDown && !growShrinkUpwards && (Mathf.Approximately(normalizedPosition.y, 0f) || normalizedPosition.y < 0f);

        // Temporarily set the pivot to only push itself and the elements above or below it, and rebuild (1)
        content.SetPivotWithoutMoving(content.pivot.WithY(growShrinkUpwards ? 0f : 1f));
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        
        // If it's not scrollable then it appears Unity uses the pivot in a special way to determine where the content should
        // be positioned within the not-yet-full viewport. (Example: a pivot.y = 0.5 causes the content to be centered vertically).
        // Changing pivot values would then cause the content to jump around the viewport while it's in the process of getting filled up.
        // We don't need to be changing the pivot here.
        if (!this.IsScrollable())
        {
            content.pivot = _onStartPivot;
            normalizedPosition = normalizedPosition.WithY(0f);
            return;
        }
        
        // Maintain our anchored position by moving the pivot (2)
        content.SetPivotWithoutMoving(initPivot);
        float diffY = content.anchoredPosition.y - initY;
        content.SetPivotWithoutMoving(content.pivot + Vector2.up * -diffY / content.rect.height);
        
        /* Special cases: */
        
        // If we went from non-fullscreen -> fullscreen then move the viewport to very beginning (i.e. show the first entries).
        // Since Unity handles ScrollRects differently if we have a full viewport or not, this bridges the gap between non-full and full viewports with consistent behaviour.
        if (!initIsScrollable)
        {
            normalizedPosition = normalizedPosition.WithY(_isTopDown ? 1f : 0f);
            return;
        }
        
        // Maintain a view of the very top
        if (shouldMaintainTopmost)
        {
            normalizedPosition = normalizedPosition.WithY(1f);
            return;
        }
        
        // Maintain a view of the very bottom
        if (shouldMaintainBotmost)
        {
            normalizedPosition = normalizedPosition.WithY(0f);
           return;
        }
    }

    /// <summary>
    /// Initializes and tracks pooled objects
    /// </summary>
    private void InitPools()
    {
        // Entry pool: unbinds all entries and starts tracking them as recycled and ready to be bound again
        _recycledEntries.Add(RecyclerScrollRectEntry<TEntryData>.UnboundIndex, new Queue<RecyclerScrollRectEntry<TEntryData>>());
        foreach (Transform child in _poolParent)
        {
            RecyclerScrollRectEntry<TEntryData> entry = child.GetComponent<RecyclerScrollRectEntry<TEntryData>>();
            entry.ResetIndex();
            entry.gameObject.SetActive(false);
            _recycledEntries[RecyclerScrollRectEntry<TEntryData>.UnboundIndex].Enqueue(entry);
        }
        
        // End-cap pool: grab a reference to the end-cap
        if (_endcapParent.childCount > 0)
        {
            _endcap = _endcapParent.GetChild(0).GetComponent<RecyclerEndcap<TEntryData>>();
            _endcap.gameObject.SetActive(false);
        }
    }
    
    private static void SetBehavioursEnabled(Behaviour[] behaviours, bool isEnabled)
    {
        Array.ForEach(behaviours, l => l.enabled = isEnabled);
    }
}