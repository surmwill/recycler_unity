using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Transform = UnityEngine.Transform;

/// <summary>
/// A scroll rect containing a list of data, only processing that data which can be seen on-screen.
/// 
/// Note: all entries and the end-cap can have auto-calculated dimensions but their widths are by default force expanded to
/// to the width of the viewport. Additionally to prevent spam layout recalculations, once calculated, all layout components
/// (ILayoutElement, ILayoutController) such as VerticalLayoutGroups get disabled; however, this also includes such things as Images.
/// In this case the Image should be moved as a child. 
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public abstract partial class RecyclerScrollRect<TEntryData, TKeyEntryData> : ScrollRect, IPointerDownHandler where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
{
    [Header("Recycler")]
    [SerializeField]
    private RecyclerScrollRectEntry<TEntryData, TKeyEntryData> _recyclerEntryPrefab = null;

    [SerializeField]
    private int _numCachedBeforeStart = 2;
    
    [SerializeField]
    private int _numCachedAfterEnd = 2;

    [SerializeField]
    private RecyclerTransformPosition _appendTo = RecyclerTransformPosition.Bot;

    [Tooltip("On mobile, the target frame rate is often lower than technically possible to preserve battery, but a higher frame rate will result in smoother scrolling.")]
    [SerializeField]
    private bool _setTargetFrameRateTo60 = true;

    [Tooltip("Perform sanity checks in the editor, ensuring for example, that we aren't skipping indices. (Note that if my code was perfect this wouldn't be needed).")]
    [SerializeField]
    private bool _debugPerformEditorChecks = true;

    [Header("Pool")]
    [SerializeField]
    private RectTransform _poolParent = null;

    [SerializeField]
    private int _poolSize = 15;

    [Header("Endcap (optional)")]
    [SerializeField]
    private RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> _endcapPrefab = null;
    
    [SerializeField]
    private RectTransform _endcapParent = null;
    
    [ReadOnly]
    [SerializeField]
    private RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> _endcap = null;

    /// <summary>
    /// Called after the Recycler's scroll has been handled and we have the correct final set of entries on screen (for this frame).
    /// Unless the user performs a manual operation (append/prepend/insert/delete), the entries will remain where they are on screen
    /// and can operated on under this assumption
    /// </summary>
    public event Action OnRecyclerUpdated;

    /// <summary>
    /// The current data being bound to the entries
    /// </summary>
    public IReadOnlyList<TEntryData> DataForEntries => _dataForEntries;

    /// <summary>
    /// The entries with active GameObjects, including both visible and cached
    /// </summary>
    public IReadOnlyDictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> ActiveEntries => _activeEntries;
    
    /// <summary>
    /// Contains information about the range of indices of active entries
    /// </summary>
    public IRecyclerScrollRectActiveEntriesWindow ActiveEntriesWindow => _activeEntriesWindow;

    /// <summary>
    /// The endcap (if it exists - it is optional)
    /// </summary>
    public RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> Endcap => _endcap;

    // In the scene hierarchy, are our entries' indices increasing as we go down the sibling list?
    // Increasing entries mean our first entry with index 0 is at the top, and so is our start cache.
    // Decreasing entries mean our first entry with index 0 is at the bottom, and so is our start cache.
    private bool AreEntriesIncreasing => _appendTo == RecyclerTransformPosition.Bot;
    
    private RecyclerTransformPosition StartCacheTransformPosition => InverseRecyclerTransformPosition(_appendTo);

    private RecyclerTransformPosition EndCacheTransformPosition => InverseRecyclerTransformPosition(StartCacheTransformPosition);

    private const float DefaultScrollSpeedViewportsPerSecond = 0.5f;

    private BoxCollider _viewportCollider = null;
    
    // All the active entries in the scene, visible and cached
    private Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> _activeEntries = new();
    
    // Previously bound entries waiting (recycled) in the pool
    private readonly RecycledEntries<TEntryData, TKeyEntryData> _recycledEntries = new();
    
    // Unbound entries waiting in the pool
    private readonly Queue<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> _unboundEntries = new();

    private readonly Dictionary<TKeyEntryData, int> _entryKeyToCurrentIndex = new();

    private readonly List<TEntryData> _dataForEntries = new();
    
    private RecyclerScrollRectActiveEntriesWindow _activeEntriesWindow;

    private Vector2 _nonFilledScrollRectPivot;

    private Coroutine _scrollToIndexCoroutine;

    private int? _currScrollingToIndex;

    protected override void Awake()
    {
        base.Awake();
        
        // The base ScrollRect has [ExecuteAlways] but the recycler does not work as such
        if (!Application.isPlaying)
        {
            return;
        }
        
        // On mobile, the target frame rate is often lower than technically possible to preserve battery, but a 
        // higher frame rate will result in smoother scrolling.
        if (_setTargetFrameRateTo60)
        {
            Application.targetFrameRate = 60;
        }
        
        // While non-fullscreen, the pivot decides how the content gets aligned in the viewport
        _nonFilledScrollRectPivot = content.pivot;
        
        // Keeps track of what indices are visible, and subsequently which indices are cached
        _activeEntriesWindow = new RecyclerScrollRectActiveEntriesWindow(_numCachedBeforeStart);

        // All the entries in the bool are initially unbound
        RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry = null;
        foreach (Transform _ in _poolParent.Children().Where(t => t.TryGetComponent(out entry)))
        {
            _unboundEntries.Enqueue(entry);
        }

        // Used to detect what entries are on screen
        InitViewportCollider();

        if (Application.isEditor)
        {
            CheckInvalidContentLayoutGroup();
        }
    }

    /// <summary>
    /// Inserts an element at the given index
    /// </summary>
    public void InsertAtIndex(int index, TEntryData entryData, FixEntries fixEntries = FixEntries.Below)
    {
        if (index < 0 || index > _dataForEntries.Count)
        {
            throw new ArgumentException($"index \"{index}\" must be >= 0 and <= the length of data \"{_dataForEntries.Count}\"");
        }
        
        // Update bookkeeping to reflect the new entry. Determine if we actually need to create it now
        InsertDataForEntryAt(index, entryData);
        
        // Not an active entry yet, it will get created when we scroll to it
        if (!_activeEntriesWindow.Contains(index))
        {
            return;
        }

        // Find the proper place in the hierarchy for the entry
        int siblingIndex = AreEntriesIncreasing ? 0 : content.childCount;
        foreach (Transform entryTransform in content)
        {
            RecyclerScrollRectEntry<TEntryData, TKeyEntryData> activeEntry = entryTransform.GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();
            if (activeEntry != null && activeEntry.Index == index - 1)
            {
                siblingIndex = activeEntry.transform.GetSiblingIndex() + (AreEntriesIncreasing ? 1 : 0);
            }
        }

        // Create the entry
        if (_activeEntriesWindow.IsInStartCache(index))
        {
            CreateAndAddEntry(index, siblingIndex, StartCacheTransformPosition == RecyclerTransformPosition.Top ? FixEntries.Below : FixEntries.Above);
        }
        else if (_activeEntriesWindow.IsInEndCache(index))
        {
            CreateAndAddEntry(index, siblingIndex, EndCacheTransformPosition == RecyclerTransformPosition.Top ? FixEntries.Below : FixEntries.Above);
        }
        else
        {
            CreateAndAddEntry(index, siblingIndex, fixEntries);
        }

        // A new entry can update the visible window and subsequently require an update of what is cached
        UpdateActiveEntries();
    }
    
    /// <summary>
    /// Inserts an element at the index corresponding to the given key
    /// </summary>
    public void InsertAtKey(TKeyEntryData insertAtKey, TEntryData entryData, FixEntries fixEntries = FixEntries.Below)
    {
        InsertAtIndex(GetCurrentIndexForKey(insertAtKey), entryData, fixEntries);
    }

    /// <summary>
    /// Inserts elements at the given index
    /// </summary>
    public void InsertRangeAtIndex(int index, IEnumerable<TEntryData> entryData, FixEntries fixEntries = FixEntries.Below)
    {
        foreach ((TEntryData entry, int i) in entryData.ZipWithIndex())
        {
            InsertAtIndex(index + i, entry, fixEntries);
        }
    }

    /// <summary>
    /// Inserts elements at the index corresponding to the given key
    /// </summary>
    public void InsertRangeAtKey(TKeyEntryData insertAtKey, IEnumerable<TEntryData> entryData, FixEntries fixEntries = FixEntries.Below)
    {
        InsertRangeAtIndex(GetCurrentIndexForKey(insertAtKey), entryData, fixEntries);
    }

    /// <summary>
    /// Removes an element at the given index
    /// </summary>
    public void RemoveAtIndex(int index, FixEntries fixEntries = FixEntries.Below)
    {
        if (index < 0 || index >= _dataForEntries.Count)
        {
            throw new ArgumentException($"index \"{index}\" must be >= 0 and < the length of data \"{_dataForEntries.Count}\"");
        }
        
        if (index == _currScrollingToIndex)
        {
            StopScrollToIndexCoroutine();
        }
        
        // Recycle the entry if it exists in the scene
        bool shouldRecycle = _activeEntriesWindow.Contains(index);
        if (shouldRecycle)
        {
            SendToRecycling(_activeEntries[index], fixEntries);
        }

        // Unbind the entry in recycling
        if (_recycledEntries.Entries.TryGetValue(index, out RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry))
        {
            entry.UnbindIndex();
            _recycledEntries.Remove(index);
            _unboundEntries.Enqueue(entry);
        }

        // Update bookkeeping to reflect the deleted entry
        RemoveDataForEntryAt(index);

        // A deleted entry can update the visible window and subsequently require an update of what is cached
        if (shouldRecycle)
        {
            UpdateActiveEntries();
        }
    }

    /// <summary>
    /// Removes an element with the given key
    /// </summary>
    public void RemoveAtKey(TKeyEntryData removeAtKey, FixEntries fixEntries = FixEntries.Below)
    {
        RemoveAtIndex(GetCurrentIndexForKey(removeAtKey), fixEntries);
    }
    
    /// <summary>
    /// Removes elements at the given index
    /// </summary>
    public void RemoveRangeAtIndex(int index, int count, FixEntries fixEntries = FixEntries.Below)
    {
        for (int i = index + count - 1; i >= index; i--)
        {
            RemoveAtIndex(index, fixEntries);
        }
    }

    /// <summary>
    /// Removes elements at the index corresponding to the given key
    /// </summary>
    public void RemoveRangeAtKey(TKeyEntryData removeAtKey, int count, FixEntries fixEntries = FixEntries.Below)
    {
        RemoveRangeAtIndex(GetCurrentIndexForKey(removeAtKey), count, fixEntries);
    }

    /// <summary>
    /// Shifts the indices of entries forward or back by 1. Update bookkeeping to reflect this
    /// </summary>
    private void ShiftIndicesBoundEntries(int startIndex, int shiftAmount)
    {
        // Shift our active entries
       Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> shiftedActiveEntries = new Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();

       foreach ((int index, RecyclerScrollRectEntry<TEntryData, TKeyEntryData> activeEntry) in _activeEntries)
       {
           int shiftedIndex = index + (index >= startIndex ? shiftAmount : 0);
           if (shiftedIndex != index)
           {
               activeEntry.SetIndex(shiftedIndex);   
           }
           shiftedActiveEntries[shiftedIndex] = activeEntry;
       }

       _activeEntries = shiftedActiveEntries;
       
       // Shift our recycled entries
       _recycledEntries.ShiftIndices(startIndex, shiftAmount);
       
       // Shift the entry we are currently scrolling to
       if (_currScrollingToIndex.HasValue && _currScrollingToIndex.Value >= startIndex)
       {
           _currScrollingToIndex += shiftAmount;
       }
    }
    
    private void ShiftKeyToIndexMapping(int index, int shiftAmount)
    {
        for (int i = index; i < _dataForEntries.Count; i++)
        {
            _entryKeyToCurrentIndex[_dataForEntries[index].Key] += shiftAmount;
        }
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
        UpdateActiveEntries();
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
        UpdateActiveEntries();

        // We now have the final set of entries in their correct positions for this frame.
        // Give the user the opportunity for to query/operate on them knowing they won't shift.
        OnRecyclerUpdated?.Invoke();
        
        // Sanity checks
        #if UNITY_EDITOR
        
        if (_debugPerformEditorChecks)
        {
            DebugCheckWindow();
            DebugCheckDuplicates();  
            DebugCheckOrdering();   
        }
        
        #endif
    }

    private void UpdateActiveEntries()
    {
        // Get the current state of visible entries
        UpdateVisibility();

        // If the window of active entries changes we'll need to update the cache accordingly
        while (_activeEntriesWindow.IsDirty)
        {
            _activeEntriesWindow.SetNonDirty();

            List<int> toRecycleEntries = new();
            List<int> newCachedStartEntries = new();
            List<int> newCachedEndEntries = new();

            // Determine what entries need to be removed (aren't in the cache and aren't visible)
            foreach ((int index, RecyclerScrollRectEntry<TEntryData, TKeyEntryData> _) in _activeEntries)
            {
                if (!_activeEntriesWindow.Contains(index))
                {
                    toRecycleEntries.Add(index);   
                }
            }
            
            // Determine what entries need to be added to the start or end cache
            if (_activeEntriesWindow.StartCacheIndexRange.HasValue)
            {
                for (int i = _activeEntriesWindow.StartCacheIndexRange.Value.End; i >= _activeEntriesWindow.StartCacheIndexRange.Value.Start; i--)
                {
                    if (!_activeEntries.ContainsKey(i))
                    {
                        newCachedStartEntries.Add(i);
                    }
                }   
            }

            if (_activeEntriesWindow.EndCacheIndexRange.HasValue)
            {
                for (int i = _activeEntriesWindow.EndCacheIndexRange.Value.Start; i <= _activeEntriesWindow.EndCacheIndexRange.Value.End; i++)
                {
                    if (!_activeEntries.ContainsKey(i))
                    {
                        newCachedEndEntries.Add(i);
                    }
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
                    if (content.GetChild(i).GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>() == null)
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
                    if (content.GetChild(i).GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>() == null)
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
        bool shouldEndcapExist = _dataForEntries.Any() && _activeEntriesWindow.Contains(_dataForEntries.Count - 1);

        // Endcap exists, see if we need to remove it
        if (endcapExists && !shouldEndcapExist)
        {
            RecycleEndcap();
        }
        // Endcap does not exist, see if we need to create it
        else if (!endcapExists && shouldEndcapExist)
        {
            _endcap.transform.SetParent(content, false);
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
        RemoveFromContent(_endcap.RectTransform, EndCacheTransformPosition == RecyclerTransformPosition.Top ? FixEntries.Below : FixEntries.Above).SetParent(_endcapParent, false);
        _endcap.OnSentToRecycling();
    }

    private void CreateAndAddEntry(int dataIndex, int siblingIndex, FixEntries fixEntries = FixEntries.Below)
    {
        Debug.Log("CREATING " + dataIndex + " " + Time.frameCount);
        
        if (!TryFetchFromRecycling(dataIndex, out RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry))
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
    }
    
    /// <summary>
    /// Updates the window of entries that are shown (which also affects what entries need to be waiting in the cache)
    /// </summary>
    private void UpdateVisibility()
    {
        foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in _activeEntries.Values)
        {
            bool isVisible = IsInViewport(entry.RectTransform);
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
        void EntryIsVisible(RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry)
        {
            int entryIndex = entry.Index;

            if (!_activeEntriesWindow.VisibleIndexRange.HasValue)
            {
                _activeEntriesWindow.VisibleIndexRange = (entryIndex, entryIndex);
                return;
            }
            
            (int Start, int End) newVisibleIndices = _activeEntriesWindow.VisibleIndexRange.Value;
            
            if (entryIndex < _activeEntriesWindow.VisibleIndexRange.Value.Start)
            {
                newVisibleIndices.Start = entryIndex;
            }
            
            if (entryIndex > _activeEntriesWindow.VisibleIndexRange.Value.End)
            {
                newVisibleIndices.End = entryIndex;
            }

            _activeEntriesWindow.VisibleIndexRange = newVisibleIndices;
        }

        // Not visible
        void EntryIsNotVisible(RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry)
        {
            if (!_activeEntriesWindow.VisibleIndexRange.HasValue)
            {
                return;
            }
            
            (int Start, int End) newVisibleIndices = _activeEntriesWindow.VisibleIndexRange.Value;
            int entryIndex = entry.Index;
            bool wentOffTop = Vector3.Dot(entry.RectTransform.position - viewport.transform.position, viewport.transform.up) > 0;
            
            // Note that for any entry to be non-visible there must be at least one other entry pushing it offscreen.
            // This means there's a guaranteed existent entry below/above it and we can be safe adding +/- 1 to our index window bounds

            // Entries are increasing (entry 0 is at the top along with our start cache)
            if (AreEntriesIncreasing)
            {
                // Anything off the top means we are scrolling down, away from entry 0, away from lesser indices
                if (wentOffTop && _activeEntriesWindow.VisibleIndexRange.Value.Start <= entryIndex)
                {
                    newVisibleIndices.Start = entryIndex + 1;
                }
                // Anything off the bot means we are scrolling up, toward entry 0, toward lesser indices
                else if (!wentOffTop && _activeEntriesWindow.VisibleIndexRange.Value.End >= entryIndex)
                {
                    newVisibleIndices.End = entryIndex - 1;
                }
            }
            // Entries are decreasing (entry 0 is at the bottom along with our start cache)
            else
            {
                // Anything off the top means we are scrolling down, toward entry 0, toward lesser indices
                if (wentOffTop && _activeEntriesWindow.VisibleIndexRange.Value.End >= entryIndex)
                {
                    newVisibleIndices.End = entryIndex - 1;
                }
                // Anything off the bottom means we are scrolling up, away from entry 0, away from lesser indices
                else if (!wentOffTop && _activeEntriesWindow.VisibleIndexRange.Value.Start <= entryIndex)
                {
                    newVisibleIndices.Start = entryIndex + 1;
                }
            }

            _activeEntriesWindow.VisibleIndexRange = newVisibleIndices;
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
    /// Returns the state of the entry at the given index
    /// </summary>
    public RecyclerScrollRectContentState GetStateOfEntryWithCurrentIndex(int index)
    {
        if (index < 0 || index >= _dataForEntries.Count)
        {
            throw new ArgumentException($"index \"{index}\" must be >= 0 and < the length of data \"{_dataForEntries.Count}\"");
        }
        
        if (_activeEntriesWindow.IsVisible(index))
        {
            return RecyclerScrollRectContentState.ActiveVisible;
        }
        
        if (_activeEntriesWindow.IsInStartCache(index))
        {
            return RecyclerScrollRectContentState.ActiveInStartCache;
        }

        if (_activeEntriesWindow.IsInEndCache(index))
        {
            return RecyclerScrollRectContentState.ActiveInEndCache;
        }

        return RecyclerScrollRectContentState.InactiveInPool;
    }

    /// <summary>
    /// Returns the state of the entry with a given key
    /// </summary>
    public RecyclerScrollRectContentState GetStateOfEntryWithKey(TKeyEntryData key)
    {
        return GetStateOfEntryWithCurrentIndex(GetCurrentIndexForKey(key));
    }

    /// <summary>
    /// Returns the state of the endcap
    /// </summary>
    public RecyclerScrollRectContentState GetStateOfEndcap()
    {
        if (!_endcap.gameObject.activeSelf)
        {
            return RecyclerScrollRectContentState.InactiveInPool;
        }
        
        if (IsInViewport(_endcap.RectTransform))
        {
            return RecyclerScrollRectContentState.ActiveVisible;
        }

        return RecyclerScrollRectContentState.ActiveInEndCache;
    }

    /// <summary>
    /// Resets the scroll rect to its initial state with no entries
    /// </summary>
    public void Clear()
    {
        // Stop any active dragging
        StopMovementAndDrag();
        
        // Stop auto-scrolling to an index
        StopScrollToIndexCoroutine();

        // Upon clearing, all entries should return to the pool unbound. We expect (and will check for) this amount of unbound entries
        #if UNITY_EDITOR
        int numTotalBoundEntries = _activeEntries.Count + _recycledEntries.Entries.Count;
        int numTargetUnboundEntries = numTotalBoundEntries + _unboundEntries.Count;
        #endif
        
        // Recycle all the entries
        foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in _activeEntries.Values.ToList())
        {
            SendToRecycling(entry);
        }

        // Unbind everything
        foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in _recycledEntries.Entries.Values.ToList())
        {
            _recycledEntries.Remove(entry.Index);
            entry.UnbindIndex();
            _unboundEntries.Enqueue(entry);
        }

        // Recycle the end-cap if it exists
        if (_endcap != null)
        {
            RecycleEndcap();
        }

        // Clear the data for the entries
        _dataForEntries.Clear();
        
        // Reset our window back to one with no entries
        _activeEntriesWindow.Reset();

        // Reset our pivot to whatever its initial value was
        content.pivot = _nonFilledScrollRectPivot;

        // Check that we have returned to the initial state
        #if UNITY_EDITOR

        if (_dataForEntries.Any())
        {
            throw new Exception("The data is supposed to cleared, but there is still some present.");
        }
        
        if (_entryKeyToCurrentIndex.Any())
        {
            throw new Exception("The data has been cleared. There should be no keys either.");
        }
        
        if (_activeEntries.Any())
        {
            throw new Exception($"The data has been cleared. We should not have any active entries.");
        }
        
        if (_activeEntriesWindow.HasData)
        {
            throw new Exception($"The data has been cleared and the window should not exist. There's no underlying data to have a window over.");
        }

        if (_recycledEntries.Entries.Any())
        {
            throw new Exception($"After clearing, all entries should return to the pool unbound. There are still {_recycledEntries.Entries.Count} entries in the pool bound.");
        }

        int numMissingUnboundEntries = numTargetUnboundEntries - _unboundEntries.Count; 
        if (numMissingUnboundEntries != 0)
        {
            throw new Exception($"After clearing, all entries should return to the pool unbound. Missing {numMissingUnboundEntries} entries.");
        }

        if (_endcap != null)
        {
            throw new Exception("The data has been cleared. We expect an empty window and therefore the endcap should not exist.");
        }

        if (_currScrollingToIndex.HasValue || _scrollToIndexCoroutine != null)
        {
            throw new Exception("The data has been cleared. We should not be auto-scrolling to an index.");
        }

        #endif
    }

    private void StopMovementAndDrag()
    {
        OnEndDrag(new PointerEventData(EventSystem.current));
        StopMovement();
    }

    private void SendToRecycling(RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry, FixEntries fixEntries = FixEntries.Below)
    {
        Debug.Log("RECYCLED: " + entry.Index + " " + Time.frameCount);

        // Handle the GameObject
        RectTransform entryTransform = entry.RectTransform;
        RemoveFromContent(entryTransform, fixEntries);
        entryTransform.SetParent(_poolParent, false);

        // Mark the entry for re-use
        if (_recycledEntries.Entries.ContainsKey(entry.Index))
        {
            throw new InvalidOperationException("We should not have two copies of the same entry in recycling, we only need one.");
        }
        _recycledEntries.Add(entry.Index, entry);

        // Bookkeeping
        _activeEntries.Remove(entry.Index);

        // Callback
        entry.OnRecycled();
    }

    private bool TryFetchFromRecycling(int entryIndex, out RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry)
    {
        entry = null;
        
        // First try to use the equivalent already bound entry waiting in recycling
        if (_recycledEntries.Entries.TryGetValue(entryIndex, out entry) )
        {
            _recycledEntries.Remove(entryIndex);
        }
        // Then try to use an unbound entry
        else if (_unboundEntries.TryDequeue(out entry))
        {
        }
        // Then try and use just any already bound entry waiting in recycling
        else if (_recycledEntries.Entries.Any())
        {
            (int firstIndex, RecyclerScrollRectEntry<TEntryData, TKeyEntryData> firstEntry) = _recycledEntries.GetOldestEntry();
            entry = firstEntry;
            _recycledEntries.Remove(firstIndex);
        }
        // Nothing to fetch from recycling, we'll have to create something new
        else
        {
            return false;
        }

        // Note: if we are using a Canvas with "Screen Space - Camera" then previously recycled entries could have a different z based on the position 
        // of the canvas at the time they were recycled. Reset this back to 0 to align with the current Canvas position.
        entry.transform.SetParent(content, false);
        entry.transform.localPosition = entry.transform.localPosition.WithZ(0f);
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
        child.SetParent(content, false);
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
        if (!IsInViewport(child))
        {
            fixEntries = IsAboveViewport(child) ? FixEntries.Below : FixEntries.Above;
        }
        
        // Remove the child and recalculate the parent's size
        child.gameObject.SetActive(false);
        RecalculateContentSize(fixEntries);

        return child;
    }

    /// <summary>
    /// Called when a child needs its dimensions updated
    /// </summary>
    private void RecalculateContentChildSize(RectTransform contentChild, FixEntries fixEntries = FixEntries.Below)
    {
        // If the child is not visible then grow in the direction which keeps it off screen and preserves the currently visible entries
        if (!IsInViewport(contentChild))
        {
            fixEntries = IsAboveViewport(contentChild) ? FixEntries.Below : FixEntries.Above;
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
    /// Called when an entry has updated its dimensions, and now the Recycler needs to update its own dimensions in turn
    /// </summary>
    public void RecalculateEntrySize(RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry, FixEntries fixEntries = FixEntries.Below)
    {
        RecalculateContentChildSize(entry.RectTransform, fixEntries);
    }

    /// <summary>
    /// Recalculates the endcaps dimensions.
    /// 
    /// Unless specified, as the endcap is at the end, we fix all entries that come before it
    /// (i.e. if the endcap is at the bottom we grow downwards, and if the endcap is a the top we grow upwards)
    /// </summary>
    public void RecalculateEndcapSize(FixEntries? fixEntries = null)
    {
        RecalculateContentChildSize(_endcap.RectTransform, fixEntries ?? (EndCacheTransformPosition == RecyclerTransformPosition.Bot ? FixEntries.Above : FixEntries.Below));
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

        // If we haven't filled up the viewport yet, there's no need to be moving the pivot around to maintain our current view (everything can already be seen)
        if (!this.IsScrollable())
        {
            content.pivot = _nonFilledScrollRectPivot;
            
            // This seems superfluous, but with < fullscreen worth of content the pivot also controls where in the viewport the (< fullscreen) content
            // is centered. This alignment does not immediately occur upon pivot change, so we trigger it immediately here by a normalized position call.
            normalizedPosition = normalizedPosition.WithY(0f);
            
            return;
        }

        // Maintain our anchored position by moving the pivot (2)
        content.SetPivotWithoutMoving(initPivot);
        float diffY = content.anchoredPosition.y - initY;
        content.SetPivotWithoutMoving(content.pivot + Vector2.up * -diffY / content.rect.height);
    }

    /// <summary>
    /// Scrolls to a given index
    /// </summary>
    public void ScrollToIndex(
         int index, 
         ScrollToAlignment scrollToAlignment = ScrollToAlignment.EntryMiddle, 
         Action onScrollComplete = null, 
         float scrollSpeedViewportsPerSecond = DefaultScrollSpeedViewportsPerSecond, 
         bool isImmediate = false)
     {
         if (index < 0 || index >= _dataForEntries.Count)
         {
             throw new ArgumentException($"index \"{index}\" must be >= 0 and < the length of data \"{_dataForEntries.Count}\"");
         }

         if (_scrollToIndexCoroutine != null)
         {
             StopScrollToIndexCoroutine();
         }

         _currScrollingToIndex = index;
         _scrollToIndexCoroutine = StartCoroutine(ScrollToIndexInner(scrollToAlignment, onScrollComplete, scrollSpeedViewportsPerSecond, isImmediate));
     }
    
    /// <summary>
    /// Scrolls to a given key
    /// </summary>
    public void ScrollToKey(
        TKeyEntryData key,
        ScrollToAlignment scrollToAlignment = ScrollToAlignment.EntryMiddle,
        Action onScrollComplete = null,
        float scrollSpeedViewportsPerSecond = DefaultScrollSpeedViewportsPerSecond,
        bool isImmediate = false)
    {
        ScrollToIndex(GetCurrentIndexForKey(key), scrollToAlignment, onScrollComplete, scrollSpeedViewportsPerSecond, isImmediate);
    }

    private IEnumerator ScrollToIndexInner(ScrollToAlignment scrollToAlignment, Action onScrollComplete, float scrollSpeedViewportsPerSecond, bool isImmediate)
     {
         // Scrolling should not fight existing movement
         StopMovementAndDrag();

         // The position within the child we will scroll to
         float normalizedPositionWithinChild = 0f;
         switch (scrollToAlignment)
         {
             case ScrollToAlignment.EntryMiddle:
                 normalizedPositionWithinChild = 0.5f;
                 break;
             
             case ScrollToAlignment.EntryTop:
                 normalizedPositionWithinChild = 1f;
                 break;
             
             case ScrollToAlignment.EntryBottom:
                 normalizedPositionWithinChild = 0f;
                 break;
         }
         
         float distanceToTravelThisFrame = GetFullDistanceToTravelInThisFrame();
         while (this.IsScrollable())
         {
             int index = _currScrollingToIndex.Value;
             
             float normalizedDistanceToTravelThisFrame = DistanceToNormalizedScrollDistance(distanceToTravelThisFrame);
             float currNormalizedY = normalizedPosition.y;
             float newNormalizedY = 0f;

             // Scroll through entries until the entry we want is created, then we'll know the exact position to scroll to
             if (!_activeEntriesWindow.Contains(index))
             {
                 // Scroll toward lesser indices
                 if (index < _activeEntriesWindow.ActiveEntriesRange.Value.Start)
                 {
                     // If the entries are increasing, then lesser entries are found at the top with a higher normalized scroll position
                     newNormalizedY = Mathf.MoveTowards(currNormalizedY, AreEntriesIncreasing ? 1 : 0, normalizedDistanceToTravelThisFrame);
                 }
                 // Scroll toward greater indices
                 else if (index > _activeEntriesWindow.ActiveEntriesRange.Value.End)
                 {
                     // If the entries are increasing, then greater entries are found at the bottom with a lower normalized scroll position
                     newNormalizedY = Mathf.MoveTowards(currNormalizedY, AreEntriesIncreasing ? 0 : 1, normalizedDistanceToTravelThisFrame);
                 }
                 
                 normalizedPosition = normalizedPosition.WithY(newNormalizedY);
             }

             // Find and scroll to the exact position of the now active entry
             else
             {
                 float entryNormalizedY = this.GetNormalizedVerticalPositionOfChild(_activeEntries[index].RectTransform, normalizedPositionWithinChild);

                 newNormalizedY = Mathf.MoveTowards(currNormalizedY, entryNormalizedY, normalizedDistanceToTravelThisFrame);
                 normalizedPosition = normalizedPosition.WithY(newNormalizedY);

                 if (this.IsAtNormalizedPosition(normalizedPosition.WithY(entryNormalizedY)))
                 {
                     break;
                 }
             }
             
             // If we didn't make any progress in our iteration we must have travelled the full frame distance - otherwise keep scrolling.
             // Note: it would be clearer to check if the distance to travel this frame is 0, but in practice it only approaches it (and outside of Mathf.Approximately)
             float distanceTravelledInIteration = NormalizedScrollDistanceToDistance(Mathf.Abs(newNormalizedY - currNormalizedY));
             if (Mathf.Approximately(distanceTravelledInIteration, 0f))
             {
                 if (!isImmediate)
                 {
                     yield return null;   
                 }
                 
                 distanceToTravelThisFrame = GetFullDistanceToTravelInThisFrame();
             }
             else
             {
                 distanceToTravelThisFrame -= distanceTravelledInIteration;
                 UpdateActiveEntries();
             }
         }
         
         _currScrollingToIndex = null;
         _scrollToIndexCoroutine = null;
         onScrollComplete?.Invoke();

         // Returns the distance we'd like to scroll in a single frame
         float GetFullDistanceToTravelInThisFrame()
         {
             return scrollSpeedViewportsPerSecond * viewport.rect.height * Time.deltaTime;
         }

         // Returns the normalized scroll distance corresponding to a certain non-normalized distance
         float DistanceToNormalizedScrollDistance(float distance)
         {
             return Mathf.InverseLerp(0, content.rect.height - viewport.rect.height, distance);
         }

         // Returns the distance corresponding to scrolling a certain normalized distance
         float NormalizedScrollDistanceToDistance(float normalizedScrollDistance)
         {
             return normalizedScrollDistance * (content.rect.height - viewport.rect.height);
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
         if (index < 0 || index > _dataForEntries.Count)
         {
             throw new IndexOutOfRangeException($"Invalid index: {index}. Current data length: {_dataForEntries.Count}");
         }
         
         // Shift the indices of existing entries that will be affected by the insertion
         ShiftIndicesBoundEntries(index, entryData.Count); 
         ShiftKeyToIndexMapping(index, entryData.Count);
         
         // Add the inserted entries to our key mapping
         foreach ((TEntryData data, int i) in entryData.ZipWithIndex())
         {
             _entryKeyToCurrentIndex[data.Key] = index + i;
         }
         
         // Actual insertion (and modification) of underlying data
         _activeEntriesWindow.InsertRange(index, entryData.Count);
        _dataForEntries.InsertRange(index, entryData);
    }

    private void RemoveDataForEntryAt(int index)
    {
        if (index < 0 || index >= _dataForEntries.Count)
        {
            throw new IndexOutOfRangeException($"Invalid index: {index}. Current data length: {_dataForEntries.Count}");
        }
        
        // Shift the indices of existing entries that will be affected by the deletion
        ShiftIndicesBoundEntries(index + 1, -1);
        ShiftKeyToIndexMapping(index + 1, -1);

        // Remove the inserted entry from our key mapping
        _entryKeyToCurrentIndex.Remove(_dataForEntries[index].Key);
        
        // Actual removal (and modification) of underlying data
        _activeEntriesWindow.Remove(index);
        _dataForEntries.RemoveAt(index);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_scrollToIndexCoroutine != null)
        {
            StopScrollToIndexCoroutine();
        }
    }

    /// <summary>
    /// Stops scrolling to an index
    /// </summary>
    public void CancelScrollTo()
    {
        if (_scrollToIndexCoroutine != null)
        {
            StopScrollToIndexCoroutine();
        }
    }

    /// <summary>
    /// Returns the current index of an entry with a given key
    /// </summary>
    public int GetCurrentIndexForKey(TKeyEntryData key)
    {
        return _entryKeyToCurrentIndex[key];
    }

    /// <summary>
    /// Returns the key of an entry at the given current index
    /// </summary>
    public TKeyEntryData GetKeyForCurrentIndex(int index)
    {
        return _dataForEntries[index].Key;
    }

    private void StopScrollToIndexCoroutine()
    {
        _currScrollingToIndex = null;
        StopCoroutine(_scrollToIndexCoroutine);
        _scrollToIndexCoroutine = null;
    }

    /// <summary>
    /// Used to detect what is visible on screen
    /// </summary>
    private void InitViewportCollider()
    {
        // The value of z is not anything special since everything is in 2D - just a healthy buffer
        _viewportCollider = GetComponent<BoxCollider>();
        _viewportCollider.size = new Vector3(viewport.rect.width, viewport.rect.height, 1f);
    }
    
    private bool IsInViewport(RectTransform rectTransform)
    {
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        return worldCorners.Any(_viewportCollider.ContainsPoint);
    }

    private bool IsAboveViewport(RectTransform rectTransform)
    {
        return Vector3.Dot(Vector3.ProjectOnPlane(rectTransform.position - viewport.position, viewport.forward), viewport.up) > 0;
    }
    
    private void CheckInvalidContentLayoutGroup()
    {
        VerticalLayoutGroup v = content.GetComponent<VerticalLayoutGroup>();
        
        if (v.childControlWidth || v.childControlHeight)
        {
            throw new Exception($"The {nameof(VerticalLayoutGroup)} on \"{content.gameObject.name}\" cannot have {nameof(v.childControlWidth)} or {nameof(v.childControlHeight)} checked - please uncheck it.\n\n" +
                           
                           $"Upon binding, all LayoutElements and LayoutControllers on an entry's root are disabled.\n" +
                           $"Reasoning: every time an entry is added, removed, or modified, the Recycler's content recalculates its size and subsequently all of its children. " +
                           $"If the children have disabled LayoutElements and LayoutControllers then no calculation occurs in that child's subtree, saving time. " +
                           $"(When entries are added, removed, or modified, it is unlikely all the other entries also change size and need this costly recalculation.)\n" +
                           $"Disabled LayoutElements and LayoutControllers will report a 0 width and 0 height, clearly something we don't want.\n\n" +
                           
                           $"It is advised (and supported) that the entry control its own width and height with a {nameof(ContentSizeFitter)}, and, apart from binding (which is already covered), alert the Recycler of its size changes through the RecalculateDimensions method.\n" +
                           $"Also note that all entries are by default expanded to meet the width of the recycler (equivalent to {nameof(v.childForceExpandWidth)}), and this behaviour need not be replicated through the layout group.\n");
        }
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