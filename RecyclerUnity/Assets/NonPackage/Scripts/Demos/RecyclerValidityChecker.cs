using System;
using System.Collections.Generic;
using System.Linq;
using RecyclerScrollRect;
using UnityEngine;

using static RecyclerScrollRect.ViewportHelpers;

/// <summary>
/// Ensures our Recycler is in the proper format each frame.
/// For example, ensuring there are no duplicate entries and ensuring the entries are properly increasing/decreasing.
/// </summary>
public class RecyclerValidityChecker<TEntryData, TKeyEntryData> where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
{
    private readonly RecyclerScrollRect<TEntryData, TKeyEntryData> _recycler;
    private readonly RectTransform _recyclerViewport;
    private readonly Canvas _rootCanvas;

    private RecyclerPosition StartCachePosition => EndCachePosition == RecyclerPosition.Bot ? RecyclerPosition.Top : RecyclerPosition.Bot;
    
    private RecyclerPosition EndCachePosition => _recycler.AppendTo;

    public RecyclerValidityChecker(RecyclerScrollRect<TEntryData, TKeyEntryData> recycler)
    {
        _recycler = recycler;
        _recyclerViewport = recycler.viewport;

        _rootCanvas = recycler.GetComponent<Canvas>();
        if (_rootCanvas == null)
        {
            _rootCanvas = recycler.GetComponentInParent<Canvas>();
        }
        _rootCanvas = _rootCanvas.rootCanvas;
    }

    /// <summary>
    /// Starts the error checking each frame
    /// </summary>
    public void Bind()
    {
        Debug.Log("Starting recycler validity checking.");
        _recycler.OnRecyclerUpdated += CheckValidity;
    }

    /// <summary>
    /// Stops the error checking
    /// </summary>
    public void Unbind()
    {
        Debug.Log("Stopping recycler validity checking.");
        _recycler.OnRecyclerUpdated -= CheckValidity;
    }

    private void CheckValidity()
    {
        // Check the window indices make sense
        DebugCheckValidWindowIndices();
        
        // Check that all the recycler children are active entries, and the active entries are 1-to-1 with the indices tracked in the window
        DebugCheckAllChildrenAreActiveEntries();
        DebugCheckWindowAlignsWithEntryPositions();

        // Check that the endcap is positioned properly
        DebugCheckEndcapPosition();
        
        // Check for duplicate entries and endcaps. Check that the entries are properly ordered
        DebugCheckDuplicates();
        DebugCheckOrdering();
        
        // Check that entries' keys map to their index (which can shift)
        DebugCheckKeyToIndexMapping();
        
        // Check that the visible state of entries and the endcap match their position in the viewport
        DebugCheckVisibility();
        
        // Check that pooled entries and endcaps fall under the correct transform
        DebugCheckPool();
    }

    /// <summary>
    /// Check that the start index of the visible indices is not > the end.
    /// </summary>
    private void DebugCheckValidWindowIndices()
    {
        (int Start, int End)? visibleIndexRange = _recycler.ActiveEntriesWindow.VisibleIndexRange;
        if (!visibleIndexRange.HasValue)
        {
            return;
        }

        if (visibleIndexRange.Value.Start > visibleIndexRange.Value.End)
        {
            Debug.LogError($"The visible start index \"{visibleIndexRange.Value.Start}\" should not be greater than the end index \"{visibleIndexRange.Value.End}\"");
            Debug.Break();
            return;
        }
    }
    
    /// <summary>
    /// Check that the children of the recycler are all active entries, and that there are no missing or extra.
    /// </summary>
    private void DebugCheckAllChildrenAreActiveEntries()
    {
        Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> activeEntries = 
            new Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>(_recycler.ActiveEntries);
        
        foreach (Transform t in _recycler.content)
        {
            RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> endcap = t.GetComponent<RecyclerScrollRectEndcap<TEntryData, TKeyEntryData>>();
            if (endcap != null)
            {
                continue;
            }
            
            RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry = t.GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();
            if (entry == null)
            {
                Debug.LogError($"{t.gameObject.name} is a child of the recycler but not an entry or endap");
                Debug.Break();
                return;
            }

            if (!activeEntries.TryGetValue(entry.Index, out RecyclerScrollRectEntry<TEntryData, TKeyEntryData> activeEntry) || entry != activeEntry)
            {
                Debug.LogError($"Entry with index \"{entry.Index}\" is present as a recycler child but not tracked as active");
                Debug.Break();
                return;
            }

            activeEntries.Remove(entry.Index);
        }

        if (activeEntries.Any())
        {
            Debug.LogError($"Entries: \"{string.Join(',', activeEntries.Keys)}\" are reported as active but not present as a child in the recycler");
        }
    }
    
    /// <summary>
    /// Check that all active entries map correctly to their state in the index window, and the index window maps
    /// correctly to all active entries.
    /// </summary>
    private void DebugCheckWindowAlignsWithEntryPositions()
    {
        HashSet<int> indicesInStartCache = new HashSet<int>();
        HashSet<int> indicesInEndCache = new HashSet<int>();
        HashSet<int> visibleIndices = new HashSet<int>();

        // Check which indices we report as visible, in the start cache, and in the end cache
        IRecyclerScrollRectActiveEntriesWindow activeEntriesWindow = _recycler.ActiveEntriesWindow;
        if (activeEntriesWindow.StartCacheIndexRange.HasValue)
        {
            (int Start, int End) = activeEntriesWindow.StartCacheIndexRange.Value;
            indicesInStartCache = new HashSet<int>(Enumerable.Range(Start, End - Start + 1));
        }

        if (activeEntriesWindow.EndCacheIndexRange.HasValue)
        {
            (int Start, int End) = activeEntriesWindow.EndCacheIndexRange.Value;
            indicesInEndCache = new HashSet<int>(Enumerable.Range(Start, End - Start + 1));
        }

        if (activeEntriesWindow.VisibleIndexRange.HasValue)
        {
            (int Start, int End) = activeEntriesWindow.VisibleIndexRange.Value;
            visibleIndices = new HashSet<int>(Enumerable.Range(Start, End - Start + 1));
        }

        foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in _recycler.ActiveEntries.Values)
        {
            // Entries that are visible in the viewport should be reported as visible
            if (IsInViewport(entry.RectTransform, _recycler.viewport, _rootCanvas.worldCamera))
            {
                if (!visibleIndices.Remove(entry.Index))
                {
                    Debug.LogError($"{entry.Index} should be in the visible index window.\n\n {activeEntriesWindow.PrintRanges()}");
                    Debug.Break();
                    return;   
                }
            }
            // Entries that are above the viewport should be reported as in the start/end cache, depending on orientation
            else if (IsAboveViewportCenter(entry.RectTransform, _recyclerViewport))
            {
                if (StartCachePosition == RecyclerPosition.Top)
                {
                    if (!indicesInStartCache.Remove(entry.Index))
                    {
                        Debug.LogError($"{entry.Index} should be in the start cache window.\n\n {activeEntriesWindow.PrintRanges()}");
                        Debug.Break();
                        return;   
                    }
                }
                else if (EndCachePosition == RecyclerPosition.Top)
                {
                    if (!indicesInEndCache.Remove(entry.Index))
                    {
                        Debug.LogError($"{entry.Index} should be in the end cache window.\n\n {activeEntriesWindow.PrintRanges()}");
                        Debug.Break();
                        return;   
                    }
                }
            }
            // Entries that are below the viewport should be reported as in the start/end cache, depending on orientation
            else
            {
                if (StartCachePosition == RecyclerPosition.Bot)
                {
                    if (!indicesInStartCache.Remove(entry.Index))
                    {
                        Debug.LogError($"{entry.Index} should be in the start cache window.\n\n {activeEntriesWindow.PrintRanges()}");
                        Debug.Break();
                        return;   
                    }
                }
                
                if (EndCachePosition == RecyclerPosition.Bot)
                {
                    if (!indicesInEndCache.Remove(entry.Index))
                    {
                        Debug.LogError($"{entry.Index} should be in the end cache window.\n\n {activeEntriesWindow.PrintRanges()}");
                        Debug.Break();
                        return;   
                    }
                }
            }
        }

        // Ensure there are no leftover indices that don't match with actual entries in the list
        if (indicesInStartCache.Any())
        {
            Debug.LogError($"The following entries were reported in the start cache window but couldn't be found in the start cache: {string.Join(',', indicesInStartCache)}");
            Debug.Break();
            return;
        }

        if (indicesInEndCache.Any())
        {
            Debug.LogError($"The following entries were reported to be in the end cache window but weren't found in the end cache: {string.Join(',', indicesInEndCache)}");
            Debug.Break();
            return;
        }

        if (visibleIndices.Any())
        {
            Debug.LogError($"The following entries were reported to be visible window but weren't found to be visible: {string.Join(',', visibleIndices)}");
            Debug.Break();
            return;
        }
    }
    
    /// <summary>
    /// Check the endcap only appears at the end of the active entries and is only active when the last index is active
    /// </summary>
    private void DebugCheckEndcapPosition()
    {
        RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> endcap = _recycler.Endcap;
        if (endcap == null)
        {
            return;
        }

        bool hasLastEntry = _recycler.ActiveEntries.ContainsKey(_recycler.DataForEntries.Count - 1);
        if (!hasLastEntry && endcap.gameObject.activeSelf)
        {
            Debug.LogError("Endcap should not be active if the last entry is not active.");
            Debug.Break();
            return;
        }
        
        if (hasLastEntry && !endcap.gameObject.activeSelf)
        {
            Debug.LogError("Endcap should be active if the last entry is active.");
            Debug.Break();
            return;
        }
        
        int endcapSiblingIndex = endcap.transform.GetSiblingIndex();
        if (EndCachePosition == RecyclerPosition.Top && endcapSiblingIndex != 0)
        {
            Debug.LogError("Endcap should be the first sibling when the end cache is at the top. No entries should come before it");
            Debug.Break();
        }
        else
        {
            Debug.LogError("Endcap should be the last sibling when the end cache is at the bottom. No entries should come after it");
            Debug.Break();
        }
    }

    /// <summary>
    /// Check for duplicate entries and endcaps
    /// </summary>
    private void DebugCheckDuplicates()
    {
        HashSet<int> seenIndices = new HashSet<int>();
        RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> foundEndcap = null;
        
        foreach (Transform t in _recycler.content)
        {
            RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> endcap = t.GetComponent<RecyclerScrollRectEndcap<TEntryData, TKeyEntryData>>();
            if (endcap != null)
            {
                if (foundEndcap != null)
                {
                    Debug.LogError("DUPLICATE ENDCAP");
                    Debug.Break();
                    return;
                }

                foundEndcap = endcap;
                continue;
            }
            
            RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry = t.GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();
            int currentIndex = entry.Index;
            
            if (seenIndices.Contains(currentIndex))
            {
                Debug.LogError($"DUPLICATE INDEX: {currentIndex}");
                Debug.Break();
                return;
            }

            seenIndices.Add(currentIndex);
        }
    }

    /// <summary>
    /// Check that the entries are in increasing/decreasing order
    /// </summary>
    private void DebugCheckOrdering()
    {
        int? lastIndex = null;
        foreach (Transform t in _recycler.content)
        {
            RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry = t.GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();
            if (entry == null)
            {
                return;
            }

            int currentIndex = entry.Index;
            if (lastIndex.HasValue && Mathf.Abs(lastIndex.Value - currentIndex) > 1f)
            {
                Debug.LogError($"Index jumped by more than one: {currentIndex}");
                Debug.Break();
                return;
            }

            lastIndex = currentIndex;
        }
    }

    /// <summary>
    /// Ensure that as we insert and remove entries and their indices shift, their keys map to their shifted index
    /// </summary>
    private void DebugCheckKeyToIndexMapping()
    {
        Dictionary<TKeyEntryData, int> _entryKeyToCurrentIndex;
        _entryKeyToCurrentIndex = GetRecyclerPrivateFieldValue<Dictionary<TKeyEntryData, int>>(nameof(_entryKeyToCurrentIndex));
        
        IReadOnlyList<TEntryData> dataForEntries = _recycler.DataForEntries;
        for (int i = 0; i < dataForEntries.Count; i++)
        {
            TKeyEntryData key = dataForEntries[i].Key;
            int mappedIndex = _entryKeyToCurrentIndex[dataForEntries[i].Key];

            if (mappedIndex != i)
            {
                Debug.LogError($"The mapped index {mappedIndex} for key \"{key}\" does not match its actual index {i}");
                Debug.Break();
                return;
            }
        }
    }

    /// <summary>
    /// Check that entries and the endcap have proper visibility values
    /// </summary>
    private void DebugCheckVisibility()
    {
        RecycledEntries<TEntryData, TKeyEntryData> _recycledEntries;
        Queue<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> _unboundEntries;
        
        _recycledEntries = GetRecyclerPrivateFieldValue<RecycledEntries<TEntryData, TKeyEntryData>>(nameof(_recycledEntries));
        _unboundEntries = GetRecyclerPrivateFieldValue<Queue<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>>(nameof(_unboundEntries));

        foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in _recycler.ActiveEntries.Values)
        {
            if (!entry.IsVisible.HasValue)
            {
                Debug.LogError($"All active entries should have a non-null {entry.IsVisible} value");
                Debug.Break();
                return;
            }
            
            bool isEntryInViewport = IsInViewport(entry.RectTransform, _recycler.viewport, _rootCanvas.worldCamera);
            if (isEntryInViewport && !entry.IsVisible.Value)
            {
                Debug.LogError($"Entry \"{entry.Index}\" is visible in the viewport but its state reports it's not visible");
                Debug.Break();
                return;
            }

            if (!isEntryInViewport && entry.IsVisible.Value)
            {
                Debug.LogError($"Entry \"{entry.Index}\" is not visible in the viewport but its state reports it's visible");
                Debug.Break();
                return;
            }
            
            break;
        }

        foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> pooledEntry in _recycledEntries.Entries.Values.Concat(_unboundEntries))
        {
            if (pooledEntry.IsVisible.HasValue)
            {
                Debug.LogError($"An entry \"{pooledEntry.Index}\" in the pool has an {nameof(pooledEntry.IsVisible)} value of non-null");
                Debug.Break();
                return;
            }
        }

        RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> endcap = _recycler.Endcap;
        if (endcap == null)
        {
            return;
        }

        if (endcap.gameObject.activeSelf)
        {
            if (!endcap.IsVisible.HasValue)
            {
                Debug.LogError($"An active endcap should have a non-null {endcap.IsVisible} value");
                Debug.Break();
                return;
            }
            
            bool isEndcapInViewport = IsInViewport(endcap.RectTransform, _recycler.viewport, _rootCanvas.worldCamera);
            if (isEndcapInViewport && !endcap.IsVisible.Value)
            {
                Debug.LogError($"The endcap is visible in the viewport but its state reports it's not visible");
                Debug.Break();
                return;
            }

            if (!isEndcapInViewport && endcap.IsVisible.Value)
            {
                Debug.LogError($"The endcap is not visible in the viewport but its state reports it's visible");
                Debug.Break();
                return;
            }   
        }
        else if (endcap.IsVisible.HasValue)
        {
            Debug.LogError($"An inactive endcap should have a null {endcap.IsVisible} value");
            Debug.Break();
            return;
        }
    }

    /// <summary>
    /// Check that pooled entries and endcaps are actually in the pool (the correct transform)
    /// </summary>
    private void DebugCheckPool()
    {
        RecycledEntries<TEntryData, TKeyEntryData> _recycledEntries;
        Queue<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> _unboundEntries;
        RectTransform _poolParent;
        RectTransform _endcapParent;

        _recycledEntries = GetRecyclerPrivateFieldValue<RecycledEntries<TEntryData, TKeyEntryData>>(nameof(_recycledEntries));
        _unboundEntries = GetRecyclerPrivateFieldValue<Queue<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>>(nameof(_unboundEntries));
        _poolParent = GetRecyclerPrivateFieldValue<RectTransform>(nameof(_poolParent));
        _endcapParent = GetRecyclerPrivateFieldValue<RectTransform>(nameof(_endcapParent));
        
        // Check that each inactive entry reports that it's waiting in the pool
        foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in _recycledEntries.Entries.Values.Concat(_unboundEntries))
        {
            if (entry.transform.parent != _poolParent)
            {
                Debug.LogError($"Entries not active in the recycler should be in the recycling pool. Entry \"{entry.Index}\" isn't.");
                Debug.Break();
                return;
            }
        }

        RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> endcap = _recycler.Endcap;
        if (endcap != null && !endcap.gameObject.activeSelf && endcap.transform.parent != _endcapParent)
        {
            Debug.LogError($"An inactive endcap should be waiting in its recycling pool.");
        }
    }
    
    private TFieldValue GetRecyclerPrivateFieldValue<TFieldValue>(string fieldName)
    {
        return RecyclerScrollRectReflectionHelpers.GetPrivateFieldValue<TFieldValue, TEntryData, TKeyEntryData>(_recycler, fieldName);
    }
}
