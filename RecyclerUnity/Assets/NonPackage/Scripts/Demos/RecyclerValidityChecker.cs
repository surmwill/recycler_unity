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
        DebugCheckWindowIndices();
        DebugCheckWindowStates();
        DebugCheckWindowAlignment();

        DebugCheckDuplicates();
        DebugCheckOrdering();

        DebugCheckIndexToKeyMapping();
        DebugCheckKeyToIndexMapping();
                
        DebugCheckStates();
    }

    /// <summary>
    /// Check that the start index of the visible indices is not > the end.
    /// </summary>
    private void DebugCheckWindowIndices()
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
    /// Check that the indices we report as visible, in the start cache, and in the end cache, correspond to actual
    /// entries that are visible, in the start cache, and in the end cache
    /// </summary>
    private void DebugCheckWindowStates()
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

        foreach (Transform t in _recycler.content)
        {
            RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry = t.GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();
            if (entry == null)
            {
                return;
            }

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
    /// Check for duplicate entries
    /// </summary>
    private void DebugCheckDuplicates()
    {
        HashSet<int> seenIndices = new HashSet<int>();
        foreach (Transform t in _recycler.content)
        {
            RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry = t.GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();
            if (entry == null)
            {
                return;
            }

            int currentIndex = entry.Index;
            if (seenIndices.Contains(currentIndex))
            {
                Debug.LogError($"DUPLICATE: {currentIndex}");
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
    /// Ensure that as we insert and remove entries and their indices shift, their shifted index still maps to the same key
    /// </summary>
    private void DebugCheckIndexToKeyMapping()
    {
        IReadOnlyList<TEntryData> dataForEntries = _recycler.DataForEntries;
        for (int i = 0; i < dataForEntries.Count; i++)
        {
            TKeyEntryData actualKey = dataForEntries[i].Key;
            TKeyEntryData mappedKey = _recycler.GetKeyForCurrentIndex(i);

            if (!EqualityComparer<TKeyEntryData>.Default.Equals(actualKey, mappedKey))
            {
                Debug.LogError($"The mapped key corresponding to index {i} \"{mappedKey}\" does not match the actual key of the data at index {i} \"{actualKey}\"");
                Debug.Break();
                return;
            }
        }
    }

    /// <summary>
    /// Ensure that as we insert and remove entries and their indices shift, their keys map to their shifted index
    /// </summary>
    private void DebugCheckKeyToIndexMapping()
    {
        IReadOnlyList<TEntryData> dataForEntries = _recycler.DataForEntries;
        for (int i = 0; i < dataForEntries.Count; i++)
        {
            TKeyEntryData key = dataForEntries[i].Key;
            int mappedIndex = _recycler.GetCurrentIndexForKey(dataForEntries[i].Key);

            if (mappedIndex != i)
            {
                Debug.LogError($"The mapped index {mappedIndex} for key \"{key}\" does not match its actual index {i}");
                Debug.Break();
                return;
            }
        }
    }

    /// <summary>
    /// Ensures that the range of active indices reported in the index window correspond to the set of actual references to active entries
    /// </summary>
    private void DebugCheckWindowAlignment()
    {
        IReadOnlyDictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> activeEntries = _recycler.ActiveEntries;
        IRecyclerScrollRectActiveEntriesWindow activeEntriesWindow = _recycler.ActiveEntriesWindow;
        
        // No indices reported and no references to active entries
        if (!activeEntriesWindow.ActiveEntriesRange.HasValue && !activeEntries.Any())
        {
            return;
        }

        // No indices reported but references to active entries
        if (!activeEntriesWindow.ActiveEntriesRange.HasValue && activeEntries.Any())
        {
            Debug.LogError("The window states there are no active indices, but we are still referencing active entries.");
            Debug.Break();
            return;
        }

        (int activeIndicesStart, int activeIndicesEnd) = activeEntriesWindow.ActiveEntriesRange.Value;

        // Check that each active index has a corresponding reference to an active entry
        for (int i = activeIndicesStart; i <= activeIndicesEnd; i++)
        {
            if (!activeEntries.ContainsKey(i))
            {
                Debug.LogError($"The window states that index {i} should be active, but there is no reference to an active entry with that index.");
                Debug.Break();
                return;
            }
        }

        // Check that each reference to an active entry has a corresponding active index
        foreach (int index in activeEntries.Keys)
        {
            if (index < activeIndicesStart || index > activeIndicesEnd)
            {
                Debug.LogError($"We have a reference to an active entry with index {index}, but the window does not contain this active index.");
                Debug.Break();
                return;
            }
        }
    }

    /// <summary>
    /// Ensure that each entry's state reflects its actual position within the recycler
    /// </summary>
    private void DebugCheckStates()
    {
        IReadOnlyDictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> activeEntries = _recycler.ActiveEntries;
        
        // Private entries that we need to get through reflection, matching their names 1-to-1
        RecycledEntries<TEntryData, TKeyEntryData> _recycledEntries;
        Queue<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> _unboundEntries;

        _recycledEntries = GetRecyclerPrivateFieldValue<RecycledEntries<TEntryData, TKeyEntryData>>(nameof(_recycledEntries));
        _unboundEntries = GetRecyclerPrivateFieldValue<Queue<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>>(nameof(_unboundEntries));

        // Check that each active entry's state reflect its actual position in the list
        foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in activeEntries.Values)
        {
            switch (entry.State)
            {
                // Visible
                case RecyclerScrollRectContentState.ActiveVisible:
                {
                    if (!IsInViewport(entry.RectTransform, _recycler.viewport, _rootCanvas.worldCamera))
                    {
                        Debug.LogError($"Entry {entry.Index} state says it's visible but its position in the list does not reflect this.");
                        Debug.Break();
                        return;
                    }
                    break;
                }

                // In start cache
                case RecyclerScrollRectContentState.ActiveInStartCache:
                {
                    if ((StartCachePosition == RecyclerPosition.Top && !IsAboveViewportCenter(entry.RectTransform, _recyclerViewport)) ||
                         (StartCachePosition == RecyclerPosition.Bot && !IsBelowViewportCenter(entry.RectTransform, _recyclerViewport)))
                    {
                        Debug.LogError($"Entry {entry.Index} state says it's in the start cache but its position in the list does not reflect this.");
                        Debug.Break();
                        return;
                    }
                    break;
                }

                // In end cache
                case RecyclerScrollRectContentState.ActiveInEndCache:
                {
                    if ((EndCachePosition == RecyclerPosition.Top && !IsAboveViewportCenter(entry.RectTransform, _recyclerViewport)) ||
                        (EndCachePosition == RecyclerPosition.Bot && !IsBelowViewportCenter(entry.RectTransform, _recyclerViewport)))
                    {
                        Debug.LogError($"Entry {entry.Index} state says it's in the end cache but its position in the list does not reflect this.");
                        Debug.Break();
                        return; 
                    }
                    break;
                }

                // In the recycling pool
                case RecyclerScrollRectContentState.InactiveInPool:
                {
                    Debug.LogError($"Entry {entry.Index} state says it's in the recycling pool but it's in the list as an active entry.");
                    Debug.Break();
                    return;
                }
            }
        }
        
        // Check that each inactive entry reports that it's waiting in the pool
        foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in _recycledEntries.Entries.Values.Concat(_unboundEntries))
        {
            if (entry.State != RecyclerScrollRectContentState.InactiveInPool)
            {
                Debug.LogError($"Inactive entries should report that they are in the recycling pool, {entry.Index} with state \"{entry.State}\" doesn't.");
                Debug.Break();
                return;
            }
        }
        
        // Check that the state contained within all the entries matches what the recycler reports its state is
        foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in activeEntries.Values.Concat(_recycledEntries.Entries.Values).Concat(_unboundEntries))
        {
            RecyclerScrollRectContentState recyclerReportedEntryState = _recycler.GetStateOfEntryWithIndex(entry.Index);
            if (recyclerReportedEntryState != entry.State)
            {
                Debug.LogError($"Mismatch between the state contained in entry {entry.Index} \"{entry.State}\" and the recycler's view on its state \"{recyclerReportedEntryState}\".");
                Debug.Break();
                return;
            }   
        }
        
        RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> endcap = _recycler.Endcap;
        if (endcap == null)
        {
            return;
        }

        // Check that the endcap's state reflects its actual position in the list
        switch (_recycler.Endcap.State)
        {
            // Visible
            case RecyclerScrollRectContentState.ActiveVisible:
            {
                if (!IsInViewport(endcap.RectTransform, _recycler.viewport, _rootCanvas.worldCamera))
                {
                    Debug.LogError("The endcap's state says it's visible but its position in the list does not reflect this.");
                    Debug.Break();
                    return;
                }
                break;
            }

            // In the end cache
            case RecyclerScrollRectContentState.ActiveInEndCache:
            {
                if (EndCachePosition == RecyclerPosition.Top && !IsAboveViewportCenter(endcap.RectTransform, _recyclerViewport) || 
                    EndCachePosition == RecyclerPosition.Bot && !IsBelowViewportCenter(endcap.RectTransform, _recyclerViewport))
                {
                    Debug.LogError("The endcap's state says it's in the end cache but its position in the list does not reflect this.");
                    Debug.Break();
                    return;
                }
                break;
            }

            // In the start cache
            case RecyclerScrollRectContentState.ActiveInStartCache:
            {
                Debug.LogError("The endcap should never be in the start cache.");
                Debug.Break();
                return;
            }

            // Pooled
            case RecyclerScrollRectContentState.InactiveInPool:
            {
                if (endcap.gameObject.activeSelf)
                {
                    Debug.LogError("The endcap should not be active while in the pool");
                    Debug.Break();
                    return;
                }
                break;
            }
        }
        
        // Check that the state of the endcap reflects its actual position in the list
        if (endcap.State == RecyclerScrollRectContentState.ActiveVisible && !IsInViewport(endcap.RectTransform, _recycler.viewport, _rootCanvas.worldCamera))
        {
            Debug.LogError("The endcap's state says it's visible but its position in the list does not reflect this.");
            Debug.Break();
            return;
        }
    }
    

    private TFieldValue GetRecyclerPrivateFieldValue<TFieldValue>(string fieldName)
    {
        return RecyclerScrollRectReflectionHelpers.GetPrivateFieldValue<TFieldValue, TEntryData, TKeyEntryData>(_recycler, fieldName);
    }
}
