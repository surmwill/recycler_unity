using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    private RecyclerTransformPosition _appendTo = RecyclerTransformPosition.Bot;

    [SerializeField]
    private RectTransform _poolParent = null;
    
    [SerializeField]
    private int _poolSize = 15;

    [SerializeField]
    private RecyclerEndcap<TEntryData> _endcapPrefab = null;

    [SerializeField]
    private RectTransform _endcapParent = null;
    
    [SerializeField]
    private RecyclerEndcap<TEntryData> _endcap = null;

    /// <summary>
    /// The current list of data we are binding to entries
    /// </summary>
    public IReadOnlyList<TEntryData> DataForEntries => _dataForEntries;
    
    // In the scene hierarchy, are our entries' indices increasing as we go down the sibling list?
    // Increasing entries mean our first entry with index 0 is at the top, and so is our start cache.
    // Decreasing entries mean our first entry with index 0 is at the bottom, and so is our start cache.
    private bool AreEntriesIncreasing => _appendTo == RecyclerTransformPosition.Bot;
    
    private RecyclerTransformPosition StartCacheTransformPosition => InverseRecyclerTransformPosition(_appendTo);

    private RecyclerTransformPosition EndCacheTransformPosition => InverseRecyclerTransformPosition(StartCacheTransformPosition);

    // All the entries: visible and cached
    private Dictionary<int, RecyclerScrollRectEntry<TEntryData>> _activeEntries = new();
    private Dictionary<int, Queue<RecyclerScrollRectEntry<TEntryData>>> _recycledEntries;
    
    private SlidingIndexWindow _indexWindow;
    
    private readonly List<TEntryData> _dataForEntries = new();

    private Vector2 _nonFilledScrollRectPivot;

    private Coroutine _scrollToCoroutine;

    protected override void Awake()
    {
        base.Awake();
        
        // The base ScrollRect has [ExecuteAlways] but the recycler does not work as such
        if (!Application.isPlaying)
        {
            return;
        }

        _nonFilledScrollRectPivot = content.pivot;
        
        _indexWindow = new SlidingIndexWindow(_numCachedBeforeStart);
        InitTrackingRecyclingPool();
    }

    /// <summary>
    /// Inserts an element at the given index. Note that this implies indices can shift
    /// </summary>
    public void Insert(int index, TEntryData entryData, FixEntries fixEntries = FixEntries.Below)
    {
         Debug.Log("INSERTED " + index);
         
        // Determine where the new insertion will be going
        bool willBeInStartCache = _indexWindow.IsInStartCache(index);
        bool willBeInEndCache = _indexWindow.IsInEndCache(index);
        bool willBeVisible = _indexWindow.IsVisible(index);
        
        // Update bookkeeping to reflect the new entry. Determine if we actually need to create it now
        InsertDataForEntryAt(index, entryData);

        // We don't need to create the entry yet, it will get created when we scroll to it
        if (_indexWindow.IsInitialized && !willBeInStartCache && !willBeInEndCache && !willBeVisible)
        {
            return;
        }

        // Find the proper place in the hierarchy for the entry
        int siblingIndex = AreEntriesIncreasing ? 0 : content.childCount;
        foreach (Transform entryTransform in content)
        {
            RecyclerScrollRectEntry<TEntryData> activeEntry = entryTransform.GetComponent<RecyclerScrollRectEntry<TEntryData>>();
            if (activeEntry != null && activeEntry.Index == index - 1)
            {
                siblingIndex = activeEntry.transform.GetSiblingIndex() + (AreEntriesIncreasing ? 1 : 0);
            }
        }

        // Create the entry
        if (willBeInStartCache)
        {
            CreateAndAddEntry(index, siblingIndex, StartCacheTransformPosition == RecyclerTransformPosition.Top ? FixEntries.Below : FixEntries.Above);
        }
        else if (willBeInEndCache)
        {
            CreateAndAddEntry(index, siblingIndex, EndCacheTransformPosition == RecyclerTransformPosition.Top ? FixEntries.Below : FixEntries.Above);
        }
        else
        {
            CreateAndAddEntry(index, siblingIndex, fixEntries);
        }
        
        // A new entry can update the visible window and subsequently require an update of what is cached
        UpdateCaches();
    }

    /// <summary>
    /// Inserts elements at the given index. Note that this implies indices can shift
    /// </summary>
    public void InsertRange(int index, IEnumerable<TEntryData> entryData, FixEntries fixEntries = FixEntries.Below)
    {
        foreach ((TEntryData entry, int i) in entryData.ZipWithIndex())
        {
            Insert(index + i, entry, fixEntries);
        }
    }

    /// <summary>
    /// Removes elements at the given index. Note that this implies indices can shift
    /// </summary>
    public void RemoveRange(int index, int count, FixEntries fixEntries = FixEntries.Below)
    {
        for (int i = index + count - 1; i >= index; i--)
        {
            RemoveAt(index, fixEntries);
        }
    }

    /// <summary>
    /// Removes an element at the given index. Note that this implies indices can shift
    /// </summary>
    public void RemoveAt(int index, FixEntries fixEntries = FixEntries.Below)
    {
        // Recycle the entry if it exists in the scene
        bool shouldRecycle = _indexWindow.Contains(index);
        if (shouldRecycle)
        {
            SendToRecycling(_activeEntries[index], fixEntries);
        }
        
        // Unbind any same bound entries that were waiting for a re-bind in recycling pool
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
        
        // Update bookkeeping to reflect the deleted entry
        RemoveDataForEntryAt(index);

        // A deleted entry can update the visible window and subsequently require an update of what is cached
        if (shouldRecycle)
        {
            UpdateCaches();
        }
    }

    /// <summary>
    /// Shifts the indices of entries forward or back by 1. Update bookkeeping to reflect this
    /// </summary>
    private void ShiftIndicesBoundEntries(int startIndex, int shiftAmount)
    {
        // Shift the indices of the active entries
        Dictionary<int, RecyclerScrollRectEntry<TEntryData>> shiftedActiveEntries = new Dictionary<int, RecyclerScrollRectEntry<TEntryData>>();
        foreach ((int index, RecyclerScrollRectEntry<TEntryData> activeEntry) in _activeEntries
                     .Where(kvp => kvp.Key != RecyclerScrollRectEntry<TEntryData>.UnboundIndex))
        {
            int shiftedIndex = index + (index >= startIndex ? shiftAmount : 0);
            if (shiftedIndex != index)
            {
                activeEntry.SetIndex(shiftedIndex);   
            }
            shiftedActiveEntries[shiftedIndex] = activeEntry;
        }
        _activeEntries = shiftedActiveEntries;
        
        
        // Shift the indices of the entries in recycling
        Dictionary<int, Queue<RecyclerScrollRectEntry<TEntryData>>> shiftedRecycledEntries = new Dictionary<int, Queue<RecyclerScrollRectEntry<TEntryData>>>();
        foreach ((int index, Queue<RecyclerScrollRectEntry<TEntryData>> recycledEntries) in _recycledEntries
                     .Where(kvp => kvp.Key != RecyclerScrollRectEntry<TEntryData>.UnboundIndex))
        {
            int shiftedIndex = index + (index >= startIndex ? shiftAmount : 0);
            if (shiftedIndex != index)
            {
                foreach (RecyclerScrollRectEntry<TEntryData> unshiftedRecycledEntry in recycledEntries)
                {
                    unshiftedRecycledEntry.SetIndex(shiftedIndex);
                }   
            }
            shiftedRecycledEntries[shiftedIndex] = recycledEntries;
        }
        _recycledEntries = shiftedRecycledEntries;
    }

    /// <summary>
    /// Adds entries to the end of the list
    /// </summary>
    public void AppendEntries(IEnumerable<TEntryData> entries)
    {
        AddEntries(entries, true);
    }

    /// <summary>
    /// Adds entries to the start of the list
    /// </summary>
    public void PrependEntries(IEnumerable<TEntryData> entries)
    {
        AddEntries(entries, false);
    }

    /// <summary>
    /// Adds additional entries to display
    /// TODO: have an option not to copy over data if it's a big list. Make this take a list then
    /// </summary>
    private void AddEntries(IEnumerable<TEntryData> newEntries, bool shouldAppend)
    {
        if (newEntries == null || !newEntries.Any())
        {
            return;
        }
        
        // First entries. Create them directly instead of being fetched from the not-yet-existent cache
        bool areInitialEntries = _dataForEntries.Count == 0;
        if (areInitialEntries)
        {
            InsertRange(0, newEntries);
            return;
        }
        
        // Entries (and cache) already exist. Since we're adding them to the end they'll get created normally via a cache request
        if (shouldAppend)
        {
            InsertDataForEntriesAt(_dataForEntries.Count, new List<TEntryData>(newEntries));
        }
        else
        {
            InsertDataForEntriesAt(0, new List<TEntryData>(newEntries.Reverse()));
        }
        
        // Sometimes something put in the cache is actually visible. In this case updating the cache will cause more entries
        // to be created until they fit into the cache proper (i.e. are off-screen)
        UpdateCaches();
    }

    protected override void LateUpdate()
    {
        // Scrolling is handled here which may shift the visible window
        base.LateUpdate();

        // The base ScrollRect has [ExecuteAlways] but the recycler does not work as such
        if (!Application.isPlaying)
        {
            return;
        }

        // Update what should be in our start or end cache
        UpdateCaches();

        // Sanity checks
        if (Application.isEditor)
        {
            DebugCheckDuplicates();  
            DebugCheckOrdering();
        }
    }

    private void UpdateCaches()
    {
        // Get the current state of visible entries
        UpdateVisibility();
        
        // If the window of active entries changes we'll need to update the cache accordingly
        if (_indexWindow.IsDirty)
        {
            _indexWindow.IsDirty = false;
            // Debug.Log(_indexWindow.PrintRange());

            List<int> toRecycleEntries = new();
            List<int> newCachedStartEntries = new();
            List<int> newCachedEndEntries = new();

            // Determine what entries need to be removed (aren't in the cache and aren't visible)
            foreach ((int index, RecyclerScrollRectEntry<TEntryData> _) in _activeEntries)
            {
                if (!_indexWindow.Contains(index))
                {
                    toRecycleEntries.Add(index);   
                }
            }
            
            // Determine what entries need to be added to the start or end cache
            for (int i = _indexWindow.VisibleStartIndex.Value - 1; i >= _indexWindow.CachedStartIndex && i >= 0; i--)
            {
                if (!_activeEntries.ContainsKey(i))
                {
                    newCachedStartEntries.Add(i);
                }
            }

            for (int i = _indexWindow.VisibleEndIndex.Value; i <= _indexWindow.CachedEndIndex && i < _dataForEntries.Count; i++)
            {
                if (!_activeEntries.ContainsKey(i))
                {
                    newCachedEndEntries.Add(i);
                }
            }

            // Recycle unneeded entries
            foreach (int index in toRecycleEntries)
            {
                SendToRecycling(_activeEntries[index]);
                _activeEntries.Remove(index);
            }
            
            // Create new entries in the start cache
            bool isStartCacheAtTop = StartCacheTransformPosition == RecyclerTransformPosition.Top;
            int siblingIndexOffset = GetNumConsecutiveNonEntries(isStartCacheAtTop);
            
            foreach (int index in newCachedStartEntries)
            {
                CreateAndAddEntry(index, isStartCacheAtTop ? siblingIndexOffset : content.childCount - siblingIndexOffset, 
                    isStartCacheAtTop ? FixEntries.Below : FixEntries.Above);
            }
            
            // Create new entries in the end cache
            bool isEndCacheAtTop = EndCacheTransformPosition == RecyclerTransformPosition.Top;
            siblingIndexOffset = GetNumConsecutiveNonEntries(isEndCacheAtTop);

            foreach (int index in newCachedEndEntries)
            {
                CreateAndAddEntry(index, isEndCacheAtTop ? siblingIndexOffset : content.childCount - siblingIndexOffset, 
                    isEndCacheAtTop ? FixEntries.Below : FixEntries.Above);
            }

            // We just added/removed entries. This may have shifted the visible window
            UpdateVisibility();
        }
        
        UpdateEndcap();

        // Returns the number of consecutive non-entries from the top or bottom of the scene hierarchy. Used to insert past endcaps
        int GetNumConsecutiveNonEntries(bool fromTop)
        {
            int numConsecutiveNonEntries = 0;
            
            // Find index of the first entry from the top
            if (fromTop)
            {
                for (int i = 0; i < content.childCount; i++)
                {
                    if (content.GetChild(i).GetComponent<RecyclerScrollRectEntry<TEntryData>>() == null)
                    {
                        numConsecutiveNonEntries++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // Find the index of the first entry from the bottom
            else
            {
                for (int i = content.childCount - 1; i >= 0; i--)
                {
                    if (content.GetChild(i).GetComponent<RecyclerScrollRectEntry<TEntryData>>() == null)
                    {
                        numConsecutiveNonEntries++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return numConsecutiveNonEntries;
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

        bool endcapExists = _endcap.gameObject.activeSelf;
        bool shouldEndcapExist = _dataForEntries.Any() && _indexWindow.Contains(_dataForEntries.Count - 1);

        // Endcap exists, see if we need to remove it
        if (endcapExists && !shouldEndcapExist)
        {
            RecycleEndcap();
        }
        // Endcap does not exist, see if we need to create it
        else if (!endcapExists && shouldEndcapExist)
        {
            _endcap.transform.SetParent(content);
            _endcap.gameObject.SetActive(true);
            _endcap.OnFetchedFromRecycling();

            AddToContent(
                _endcap.RectTransform,
                AreEntriesIncreasing ? content.childCount : 0,
                EndCacheTransformPosition == RecyclerTransformPosition.Top ? FixEntries.Below : FixEntries.Above);
        }
    }

    private void RecycleEndcap()
    {
        RemoveFromContent(_endcap.RectTransform, EndCacheTransformPosition == RecyclerTransformPosition.Top ? FixEntries.Below : FixEntries.Above).SetParent(_endcapParent);
        _endcap.OnSentToRecycling();
    }

    private RecyclerScrollRectEntry<TEntryData> CreateAndAddEntry(int dataIndex, int siblingIndex, FixEntries fixEntries = FixEntries.Below)
    {
        Debug.Log("CREATING " + dataIndex);
        
        if (!TryFetchFromRecycling(dataIndex, out RecyclerScrollRectEntry<TEntryData> entry))
        {
            entry = Instantiate(_recyclerEntryPrefab, content);
        }
        
        if (entry.Index != dataIndex)
        {
            entry.BindNewData(dataIndex, _dataForEntries[dataIndex]);
        }
        else
        {
            entry.RebindExistingData();
        }
        
        AddToContent(entry.RectTransform, siblingIndex, fixEntries);
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
            int entryIndex = entry.Index;

            if (!_indexWindow.VisibleStartIndex.HasValue || entryIndex < _indexWindow.VisibleStartIndex)
            {
                _indexWindow.VisibleStartIndex = entryIndex;
            }
            
            if (!_indexWindow.VisibleEndIndex.HasValue || entryIndex > _indexWindow.VisibleEndIndex)
            {
                _indexWindow.VisibleEndIndex = entryIndex;
            }
        }

        // Not visible
        void EntryIsNotVisible(RecyclerScrollRectEntry<TEntryData> entry)
        {
            int entryIndex = entry.Index;

            // TODO: how does this handle horizontally?
            bool wentOffTop = entry.RectTransform.position.y > viewport.transform.position.y;
            
            // Entries are increasing (entry 0 is at the top along with our start cache)
            if (AreEntriesIncreasing)
            {
                // Anything off the top means we are scrolling down, away from entry 0, away from lesser indices
                if (wentOffTop && _indexWindow.VisibleStartIndex <= entryIndex)
                {
                    _indexWindow.VisibleStartIndex = entryIndex + 1;
                }
                // Anything off the bot means we are scrolling up, toward entry 0, toward lesser indices
                else if (!wentOffTop && _indexWindow.VisibleEndIndex >= entryIndex)
                {
                    _indexWindow.VisibleEndIndex = entryIndex - 1;
                }
            }
            // Entries are decreasing (entry 0 is at the bottom along with our start cache)
            else
            {
                // Anything off the top means we are scrolling down, toward entry 0, toward lesser indices
                if (wentOffTop && _indexWindow.VisibleEndIndex >= entryIndex)
                {
                    _indexWindow.VisibleEndIndex = entryIndex - 1;
                }
                // Anything off the bottom means we are scrolling up, away from entry 0, away from lesser indices
                else if (!wentOffTop && _indexWindow.VisibleStartIndex <= entryIndex)
                {
                    _indexWindow.VisibleStartIndex = entryIndex + 1;
                }
            }
        }
    }

    /// <summary>
    /// Shows the beginning of the list
    /// </summary>
    public void ResetToBeginning()
    {
        List<TEntryData> entryData = _dataForEntries.ToList();
        Clear();
        AppendEntries(entryData);
    }
    
    /// <summary>
    /// Resets the scroll rect to its initial state with no entries
    /// </summary>
    public void Clear()
    {
        // Stop any active dragging
        StopMovementAndDrag();
        
        // Recycle everything
        for (int i = content.transform.childCount - 1; i >= 0; i--)
        {
            RecyclerScrollRectEntry<TEntryData> entry = content.GetChild(i).GetComponent<RecyclerScrollRectEntry<TEntryData>>();
            if (entry != null)
            {
                SendToRecycling(entry);
                entry.ResetIndex();
            }
        }
        InitTrackingRecyclingPool();
        
        // Recycle the end-cap if it exists
        RecycleEndcap();
        
        // Clear entries
        _activeEntries.Clear();
        _recycledEntries.Clear();

        // Clear the data for the entries
        _dataForEntries.Clear();

        // Reset our pivot to whatever its initial value was
        normalizedPosition = normalizedPosition.WithY(0f);
        content.pivot = _nonFilledScrollRectPivot;

        // Reset our window back to one with no entries
        _indexWindow = new SlidingIndexWindow(_numCachedBeforeStart);
    }

    private void StopMovementAndDrag()
    {
        OnEndDrag(new PointerEventData(EventSystem.current));
        StopMovement();
    }

    private void SendToRecycling(RecyclerScrollRectEntry<TEntryData> entry, FixEntries fixEntries = FixEntries.Below)
    {
        Debug.Log("RECYCLED: " + entry.Index + " " + Time.frameCount);
        
        // Handle the GameObject
        RectTransform entryTransform = entry.RectTransform;
        RemoveFromContent(entryTransform, fixEntries);
        entryTransform.SetParent(_poolParent);

        // Mark the entry for re-use
        if (!_recycledEntries.TryGetValue(entry.Index, out Queue<RecyclerScrollRectEntry<TEntryData>> recycled))
        {
            recycled = new Queue<RecyclerScrollRectEntry<TEntryData>>();
            _recycledEntries.Add(entry.Index, recycled);
        }
        
        // Bookkeeping
        recycled.Enqueue(entry);
        _activeEntries.Remove(entry.Index);

        // Callback
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
    private void AddToContent(RectTransform child, int siblingIndex, FixEntries fixEntries = FixEntries.Below)
    {
        Behaviour[] layoutBehaviours = LayoutUtilities.GetLayoutBehaviours(child.gameObject, true);

        // Proper hierarchy
        child.SetParent(content);
        child.SetSiblingIndex(siblingIndex);
        
        // Force expand the width
        (child.anchorMin, child.anchorMax) = (Vector2.one * 0.5f, Vector2.one * 0.5f);
        child.sizeDelta = child.sizeDelta.WithX(viewport.rect.width);
        
        // Auto-calculate the height given the width, then disable layout behaviours to prevent spam recalculations
        SetBehavioursEnabled(layoutBehaviours, true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(child);
        SetBehavioursEnabled(layoutBehaviours, false);
        
        // Now calculate the change in parent size given the child's size
        RecalculateContentSize(fixEntries);
    }

    private RectTransform RemoveFromContent(RectTransform child, FixEntries fixEntries = FixEntries.Below)
    {
        // If the child is not visible then shrink in the direction which keeps it off screen and preserves the currently visible entries
        if (!child.Overlaps(viewport))
        {
            fixEntries = child.GetWorldRect().Center.y > viewport.GetWorldRect().Center.y ? FixEntries.Below : FixEntries.Above;
        }
        
        // Remove the child and recalculate the parent's size
        child.gameObject.SetActive(false);
        RecalculateContentSize(fixEntries);

        return child;
    }

    /// <summary>
    /// Called when a child needs its dimensions updated
    /// </summary>
    public void RecalculateContentChildSize(RectTransform contentChild, FixEntries fixEntries = FixEntries.Below)
    {
        Assert.IsTrue(contentChild.transform.parent == content);

        // If the child is not visible then grow in the direction which keeps it off screen and preserves the currently visible entries
        if (!contentChild.Overlaps(viewport))
        {
            fixEntries = contentChild.GetWorldRect().Center.y > viewport.GetWorldRect().Center.y ? FixEntries.Below : FixEntries.Above;
        }

        // Children control their own height (see AddToContent)
        Behaviour[] layoutBehaviours = LayoutUtilities.GetLayoutBehaviours(contentChild.gameObject, true);
        SetBehavioursEnabled(layoutBehaviours, true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentChild);
        SetBehavioursEnabled(layoutBehaviours, false);
        
        // Now calculate the change in parent size given the child's size
        RecalculateContentSize(fixEntries);
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
    private void RecalculateContentSize(FixEntries fixEntries)
    {
        // Initial state
        Vector2 initPivot = content.pivot;
        float initY = content.anchoredPosition.y;

        // Temporarily set the pivot to only push itself and the elements above or below it, and rebuild (1)
        content.SetPivotWithoutMoving(content.pivot.WithY(fixEntries == FixEntries.Below ? 0f : fixEntries == FixEntries.Above ? 1f : 0.5f));
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        
        // If it's not scrollable then it appears Unity uses the pivot in a special way to determine where the content should
        // be positioned within the not-yet-full viewport. (Example: a pivot.y = 0.5 causes the content to be centered vertically).
        // Changing pivot values would then cause the content to jump around the viewport while it's in the process of getting filled up.
        // We don't need to be changing the pivot here.
        if (!this.IsScrollable())
        {
            content.pivot = _nonFilledScrollRectPivot;
            return;
        }
        
        // Maintain our anchored position by moving the pivot (2)
        content.SetPivotWithoutMoving(initPivot);
        float diffY = content.anchoredPosition.y - initY;
        content.SetPivotWithoutMoving(content.pivot + Vector2.up * -diffY / content.rect.height);
    }

     public void ScrollToIndex(int index, ScrollToAlignment scrollToAlignment = ScrollToAlignment.EntryMiddle, Action onScrollComplete = null, float scrollSpeed = 0.05f, bool isImmediate = false)
     {
         if (_scrollToCoroutine != null)
         {
             StopCoroutine(_scrollToCoroutine);
         }

         _scrollToCoroutine = StartCoroutine(ScrollToIndexInner(index, scrollToAlignment, onScrollComplete, scrollSpeed, isImmediate));
     }
     
     /// <summary>
     /// TODO: Cache the index. Shift it if we insert. Delete it and stop scrolling if wee delete it
     /// TODO: make a SmoothDamp version of this once we've scrolled to the point where the entry is active
     /// TODO: scroll to top middle or bottom of entry
     /// TODO: scroll to -1 to go to endcap
     /// TODO: test if ineumerator movenext returns false on completion of a coroutine
     /// </summary>
     /// <param name="index"></param>
     /// <param name="scrollSpeed"></param>
     /// <returns></returns>
     private IEnumerator ScrollToIndexInner(int index, ScrollToAlignment scrollToAlignment, Action onScrollComplete, float scrollSpeed, bool isImmediate)
     {
         // Scrolling should not fight existing movement
         StopMovementAndDrag();

         // The position within the child we will scroll to
         Vector2 normalizedPositionWithinChild = Vector2.zero;
         switch (scrollToAlignment)
         {
             case ScrollToAlignment.EntryMiddle:
                 normalizedPositionWithinChild = new Vector2(0.5f, 0.5f) ;
                 break;
             
             case ScrollToAlignment.EntryTop:
                 normalizedPositionWithinChild = new Vector2(0.5f, 1f);
                 break;
             
             case ScrollToAlignment.EntryBottom:
                 normalizedPositionWithinChild = new Vector2(0.5f, 0f);
                 break;
         }

         for (;;)
         {
             // Scroll through entries until the entry we want is created, then we'll know the exact position to scroll to
             if (!_indexWindow.Contains(index))
             {
                 // Scroll toward lesser indices
                 if (index < _indexWindow.CachedStartIndex)
                 {
                     // If the entries are increasing, then lesser entries are found at the top with a higher normalized scroll position
                     normalizedPosition = Vector2.MoveTowards(normalizedPosition, normalizedPosition.WithY(AreEntriesIncreasing ? 1 : 0), scrollSpeed);
                 }
                 // Scroll toward greater indices
                 else if (index > _indexWindow.CachedEndIndex)
                 {
                     // If the entries are increasing, then greater entries are found at the bottom with a lower normalized scroll position
                     normalizedPosition = Vector2.MoveTowards(normalizedPosition, normalizedPosition.WithY(AreEntriesIncreasing ? 0 : 1), scrollSpeed);
                 }

                 if (isImmediate)
                 {
                     UpdateCaches();
                 }
                 else
                 {
                     yield return null;   
                 }
             }

             // Find and scroll to the exact position of the entry
             if (_indexWindow.Contains(index))
             {
                 Vector2 entryNormalizedScrollPos = this.GetNormalizedScrollPositionOfChild(_activeEntries[index].RectTransform, normalizedPositionWithinChild);
                 normalizedPosition = Vector2.MoveTowards(normalizedPosition, entryNormalizedScrollPos, scrollSpeed);

                 if (this.IsAtNormalizedPosition(entryNormalizedScrollPos))
                 {
                     onScrollComplete?.Invoke();
                     yield break;
                 }

                 if (isImmediate)
                 {
                     UpdateCaches();
                 }
                 else
                 {
                     yield return null;
                 }
             }
         }
     }

     /// <summary>
     /// Determines the number of pooled objects available and makes them available for binding
     /// </summary>
     private void InitTrackingRecyclingPool()
     {
         _recycledEntries = new Dictionary<int, Queue<RecyclerScrollRectEntry<TEntryData>>>
         {
             [RecyclerScrollRectEntry<TEntryData>.UnboundIndex] = new()
         };

         foreach (Transform child in _poolParent) 
         { 
             RecyclerScrollRectEntry<TEntryData> entry = child.GetComponent<RecyclerScrollRectEntry<TEntryData>>();
             _recycledEntries[RecyclerScrollRectEntry<TEntryData>.UnboundIndex].Enqueue(entry); 
         }
     }
     
     private void InsertDataForEntryAt(int index, TEntryData entryData) 
     {
         InsertDataForEntriesAt(index, new [] { entryData });
     }
     
     /// <summary>
     /// Inserts data for a new entry in the list, possibly also switching around the indices of currently bound entries.
     /// Note that this only updates bookkeeping, if the entry should also be created, that must be done separately
     /// </summary>
     private void InsertDataForEntriesAt(int index, IReadOnlyCollection<TEntryData> entryData) 
     {
        if (index < _dataForEntries.Count)
        {
            ShiftIndicesBoundEntries(index, entryData.Count);
        }
        
        _indexWindow.InsertRange(index, entryData.Count);
        _dataForEntries.InsertRange(index, entryData);
    }

    private void RemoveDataForEntryAt(int index)
    {
        if (index < _dataForEntries.Count)
        {
            ShiftIndicesBoundEntries(index, -1);
        }
        
        _indexWindow.Remove(index);
        _dataForEntries.RemoveAt(index);
    }
    
    private static void SetBehavioursEnabled(Behaviour[] behaviours, bool isEnabled)
    {
        Array.ForEach(behaviours, l => l.enabled = isEnabled);
    }

    private static RecyclerTransformPosition InverseRecyclerTransformPosition(RecyclerTransformPosition position)
    {
        switch (position)
        {
            case RecyclerTransformPosition.Bot:
                return RecyclerTransformPosition.Top;
            
            case RecyclerTransformPosition.Top:
                return RecyclerTransformPosition.Bot;
            
            default:
                throw new InvalidOperationException();
        }
    }
}