using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Swill.Recycler.ViewportHelpers;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Ensures our Recycler is in the proper format each frame.
    /// For example, ensuring there are no duplicate entries and ensuring the entries are properly increasing/decreasing.
    /// </summary>
    public class RecyclerValidityChecker<TKeyEntryData, TEntryData> where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        private readonly RecyclerScrollRect<TKeyEntryData, TEntryData> _recycler;
        private readonly RectTransform _recyclerViewport;
        private readonly Canvas _rootCanvas;

        public RecyclerValidityChecker(RecyclerScrollRect<TKeyEntryData, TEntryData> recycler)
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
            #if UNITY_EDITOR
            TestRecyclerEditorLogger.Log("Starting recycler validity checking.");
            _recycler.OnRecyclerUpdated += CheckValidity;
            #endif
        }

        /// <summary>
        /// Stops the error checking
        /// </summary>
        public void Unbind()
        {
            #if UNITY_EDITOR
            TestRecyclerEditorLogger.Log("Stopping recycler validity checking.");
            _recycler.OnRecyclerUpdated -= CheckValidity;
            #endif
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
                TestRecyclerEditorLogger.LogErrorAndBreak($"The visible start index \"{visibleIndexRange.Value.Start}\" should not be greater than the end index \"{visibleIndexRange.Value.End}\"");
            }
        }

        /// <summary>
        /// Check that the children of the recycler are all active entries, and that there are no missing or extra.
        /// </summary>
        private void DebugCheckAllChildrenAreActiveEntries()
        {
            Dictionary<int, RecyclerScrollRectEntry<TKeyEntryData, TEntryData>> activeEntries =
                new Dictionary<int, RecyclerScrollRectEntry<TKeyEntryData, TEntryData>>(_recycler.ActiveEntries);

            foreach (Transform t in _recycler.content)
            {
                RecyclerScrollRectEndcap<TKeyEntryData, TEntryData> endcap = t.GetComponent<RecyclerScrollRectEndcap<TKeyEntryData, TEntryData>>();
                if (endcap != null)
                {
                    continue;
                }

                RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry = t.GetComponent<RecyclerScrollRectEntry<TKeyEntryData, TEntryData>>();
                if (entry == null)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"{t.gameObject.name} is a child of the recycler but not an entry or endap");
                    return;
                }

                if (!activeEntries.TryGetValue(entry.Index, out RecyclerScrollRectEntry<TKeyEntryData, TEntryData> activeEntry) || entry != activeEntry)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"Entry with index \"{entry.Index}\" is present as a recycler child but not tracked as active");
                    return;
                }

                activeEntries.Remove(entry.Index);
            }

            if (activeEntries.Any())
            {
                TestRecyclerEditorLogger.LogErrorAndBreak($"Entries: \"{string.Join(',', activeEntries.Keys)}\" are reported as active but not present as a child in the recycler");
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
            IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> activeEntriesWindow = _recycler.ActiveEntriesWindow;
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

            foreach (RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry in _recycler.ActiveEntries.Values)
            {
                // Entries that are visible in the viewport should be reported as visible
                if (IsInViewport(entry.RectTransform, _recycler.viewport, _rootCanvas.worldCamera))
                {
                    if (!visibleIndices.Remove(entry.Index))
                    {
                        TestRecyclerEditorLogger.LogErrorAndBreak($"{entry.Index} should be in the visible index window.\n\n {activeEntriesWindow.PrintRanges()}");
                        return;
                    }
                }
                // Entries that are above the viewport should be reported as in the start/end cache, depending on orientation
                else if (IsAboveViewportCenter(entry.RectTransform, _recyclerViewport))
                {
                    if (_recycler.Orientation == RecyclerScrollRectOrientation.TopToBottom)
                    {
                        if (!indicesInStartCache.Remove(entry.Index))
                        {
                            TestRecyclerEditorLogger.LogErrorAndBreak($"{entry.Index} should be in the start cache window.\n\n {activeEntriesWindow.PrintRanges()}");
                            return;
                        }
                    }
                    else if (_recycler.Orientation == RecyclerScrollRectOrientation.BottomToTop)
                    {
                        if (!indicesInEndCache.Remove(entry.Index))
                        {
                            TestRecyclerEditorLogger.LogErrorAndBreak($"{entry.Index} should be in the end cache window.\n\n {activeEntriesWindow.PrintRanges()}");
                            return;
                        }
                    }
                }
                // Entries that are below the viewport should be reported as in the start/end cache, depending on orientation
                else
                {
                    if (_recycler.Orientation == RecyclerScrollRectOrientation.BottomToTop)
                    {
                        if (!indicesInStartCache.Remove(entry.Index))
                        {
                            TestRecyclerEditorLogger.LogErrorAndBreak($"{entry.Index} should be in the start cache window.\n\n {activeEntriesWindow.PrintRanges()}");
                            return;
                        }
                    }

                    if (_recycler.Orientation == RecyclerScrollRectOrientation.TopToBottom)
                    {
                        if (!indicesInEndCache.Remove(entry.Index))
                        {
                            TestRecyclerEditorLogger.LogErrorAndBreak($"{entry.Index} should be in the end cache window.\n\n {activeEntriesWindow.PrintRanges()}");
                            return;
                        }
                    }
                }
            }

            // Ensure there are no leftover indices that don't match with actual entries in the list
            if (indicesInStartCache.Any())
            {
                TestRecyclerEditorLogger.LogErrorAndBreak($"The following entries were reported in the start cache window but couldn't be found in the start cache: {string.Join(',', indicesInStartCache)}");
                return;
            }

            if (indicesInEndCache.Any())
            {
                TestRecyclerEditorLogger.LogErrorAndBreak($"The following entries were reported to be in the end cache window but weren't found in the end cache: {string.Join(',', indicesInEndCache)}");
                return;
            }

            if (visibleIndices.Any())
            {
                TestRecyclerEditorLogger.LogErrorAndBreak($"The following entries were reported to be visible window but weren't found to be visible: {string.Join(',', visibleIndices)}");
            }
        }

        /// <summary>
        /// Check the endcap only appears at the end of the active entries and is only active when the last index is active
        /// </summary>
        private void DebugCheckEndcapPosition()
        {
            RecyclerScrollRectEndcap<TKeyEntryData, TEntryData> endcap = _recycler.Endcap;
            if (endcap == null)
            {
                return;
            }

            bool hasLastEntry = _recycler.ActiveEntries.ContainsKey(_recycler.DataForEntries.Count - 1);
            bool isEndcapActive = endcap.gameObject.activeInHierarchy;

            if (!hasLastEntry && isEndcapActive)
            {
                TestRecyclerEditorLogger.LogErrorAndBreak("Endcap should not be active if the last entry is not active.");
                return;
            }

            if (hasLastEntry && !isEndcapActive)
            {
                TestRecyclerEditorLogger.LogErrorAndBreak("Endcap should be active if the last entry is active.");
                return;
            }

            if (isEndcapActive)
            {
                int endcapSiblingIndex = endcap.transform.GetSiblingIndex();
                if (_recycler.Orientation == RecyclerScrollRectOrientation.TopToBottom && endcapSiblingIndex != _recycler.content.childCount - 1)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak("Endcap should be the first sibling when the end cache is at the top. No entries should come before it");
                }
                else if (_recycler.Orientation == RecyclerScrollRectOrientation.BottomToTop && endcapSiblingIndex != 0)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak("Endcap should be the last sibling when the end cache is at the bottom. No entries should come after it");
                }
            }
        }

        /// <summary>
        /// Check for duplicate entries and endcaps
        /// </summary>
        private void DebugCheckDuplicates()
        {
            HashSet<int> seenIndices = new HashSet<int>();
            RecyclerScrollRectEndcap<TKeyEntryData, TEntryData> foundEndcap = null;

            foreach (Transform t in _recycler.content)
            {
                RecyclerScrollRectEndcap<TKeyEntryData, TEntryData> endcap = t.GetComponent<RecyclerScrollRectEndcap<TKeyEntryData, TEntryData>>();
                if (endcap != null)
                {
                    if (foundEndcap != null)
                    {
                        TestRecyclerEditorLogger.LogErrorAndBreak("DUPLICATE ENDCAP");
                        return;
                    }

                    foundEndcap = endcap;
                    continue;
                }

                RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry =
                    t.GetComponent<RecyclerScrollRectEntry<TKeyEntryData, TEntryData>>();
                int currentIndex = entry.Index;

                if (seenIndices.Contains(currentIndex))
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"DUPLICATE INDEX: {currentIndex}");
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
                RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry = t.GetComponent<RecyclerScrollRectEntry<TKeyEntryData, TEntryData>>();
                if (entry == null)
                {
                    return;
                }

                int currentIndex = entry.Index;
                if (lastIndex.HasValue && Mathf.Abs(lastIndex.Value - currentIndex) > 1f)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"Index jumped by more than one: {currentIndex}");
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

            // Check correct key to index mapping
            IReadOnlyList<TEntryData> dataForEntries = _recycler.DataForEntries;
            for (int i = 0; i < dataForEntries.Count; i++)
            {
                TKeyEntryData key = dataForEntries[i].Key;
                int mappedIndex = _entryKeyToCurrentIndex[dataForEntries[i].Key];

                if (mappedIndex != i)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"The mapped index {mappedIndex} for key \"{key}\" does not match its actual index {i}");
                    return;
                }
            }

            IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> activeEntriesWindow = _recycler.ActiveEntriesWindow;

            // Check correct keys are reported as active
            CheckRange(
                activeEntriesWindow.ActiveEntriesRange,
                new HashSet<TKeyEntryData>(activeEntriesWindow.GetActiveKeys()),
                (activeIndex, activeKey) => $"Entry at index {activeIndex} has key {activeKey} that is not reported in the active keys",
                (activeKeys) => $"Keys {string.Join(',', activeKeys)} are reported as active but aren't actually");

            // Check correct keys are reported as visible
            CheckRange(
                activeEntriesWindow.VisibleIndexRange,
                new HashSet<TKeyEntryData>(activeEntriesWindow.GetVisibleKeys()),
                (visibleIndex, visibleKey) => $"Entry at index {visibleIndex} has key {visibleKey} that is not reported in the visible keys",
                (visibleKeys) => $"Keys {string.Join(',', visibleKeys)} are reported as visible but aren't actually");

            // Check correct keys are reported as in the start cache
            CheckRange(
                activeEntriesWindow.StartCacheIndexRange,
                new HashSet<TKeyEntryData>(activeEntriesWindow.GetStartCacheKeys()),
                (startCacheIndex, startCacheKey) => $"Entry at index {startCacheIndex} has key {startCacheKey} that is not reported in the start cache keys",
                (startCacheKeys) => $"Keys {string.Join(',', startCacheKeys)} are reported as in the start cache but aren't actually");

            // Check correct keys are reported as in the end cache
            CheckRange(
                activeEntriesWindow.EndCacheIndexRange,
                new HashSet<TKeyEntryData>(activeEntriesWindow.GetEndCacheKeys()),
                (endCacheIndex, endCacheKey) => $"Entry at index {endCacheIndex} has key {endCacheKey} that is not reported in the end cache keys",
                (endCacheKeys) => $"Keys {string.Join(',', endCacheKeys)} are reported as in the end cache but aren't actually");


            // Check that the given range of keys maps to the given range of indices
            void CheckRange(
                (int Start, int End)? indexRange,
                HashSet<TKeyEntryData> keysRange,
                Func<int, TKeyEntryData, string> errorIndexButNoKey,
                Func<IEnumerable<TKeyEntryData>, string> errorKeyButNoIndex)
            {
                if (indexRange.HasValue)
                {
                    (int Start, int End) = indexRange.Value;
                    foreach (int indexInRange in Enumerable.Range(Start, End - Start + 1))
                    {
                        TKeyEntryData key = _recycler.DataForEntries[indexInRange].Key;
                        if (!keysRange.Remove(key))
                        {
                            TestRecyclerEditorLogger.LogErrorAndBreak(errorIndexButNoKey.Invoke(indexInRange, key));
                            return;
                        }
                    }
                }

                if (keysRange.Any())
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak(errorKeyButNoIndex.Invoke(keysRange));
                }
            }
        }

        /// <summary>
        /// Check that entries and the endcap have proper visibility values
        /// </summary>
        private void DebugCheckVisibility()
        {
            RecycledEntries<TKeyEntryData, TEntryData> _recycledEntries;
            Queue<RecyclerScrollRectEntry<TKeyEntryData, TEntryData>> _unboundEntries;

            _recycledEntries = GetRecyclerPrivateFieldValue<RecycledEntries<TKeyEntryData, TEntryData>>(nameof(_recycledEntries));
            _unboundEntries = GetRecyclerPrivateFieldValue<Queue<RecyclerScrollRectEntry<TKeyEntryData, TEntryData>>>(nameof(_unboundEntries));

            foreach (RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry in _recycler.ActiveEntries.Values)
            {
                if (!entry.IsVisible.HasValue)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"All active entries should have a non-null {entry.IsVisible} value");
                    return;
                }

                bool isEntryInViewport = IsInViewport(entry.RectTransform, _recycler.viewport, _rootCanvas.worldCamera);
                if (isEntryInViewport && !entry.IsVisible.Value)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"Entry \"{entry.Index}\" is visible in the viewport but its state reports it's not visible");
                    return;
                }

                if (!isEntryInViewport && entry.IsVisible.Value)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"Entry \"{entry.Index}\" is not visible in the viewport but its state reports it's visible");
                    return;
                }

                break;
            }

            foreach (RecyclerScrollRectEntry<TKeyEntryData, TEntryData> pooledEntry in _recycledEntries.Entries.Values.Concat(_unboundEntries))
            {
                if (pooledEntry.IsVisible.HasValue)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"An entry \"{pooledEntry.Index}\" in the pool has an {nameof(pooledEntry.IsVisible)} value of non-null");
                    return;
                }
            }

            RecyclerScrollRectEndcap<TKeyEntryData, TEntryData> endcap = _recycler.Endcap;
            if (endcap == null)
            {
                return;
            }

            if (endcap.gameObject.activeInHierarchy)
            {
                if (!endcap.IsVisible.HasValue)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"An active endcap should have a non-null {endcap.IsVisible} value");
                    return;
                }

                bool isEndcapInViewport = IsInViewport(endcap.RectTransform, _recycler.viewport, _rootCanvas.worldCamera);
                if (isEndcapInViewport && !endcap.IsVisible.Value)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"The endcap is visible in the viewport but its state reports it's not visible");
                    return;
                }

                if (!isEndcapInViewport && endcap.IsVisible.Value)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"The endcap is not visible in the viewport but its state reports it's visible");
                }
            }
            else if (endcap.IsVisible.HasValue)
            {
                TestRecyclerEditorLogger.LogErrorAndBreak($"An inactive endcap should have a null {endcap.IsVisible} value");
            }
        }

        /// <summary>
        /// Check that pooled entries and endcaps are actually in the pool (the correct transform)
        /// </summary>
        private void DebugCheckPool()
        {
            RecycledEntries<TKeyEntryData, TEntryData> _recycledEntries;
            Queue<RecyclerScrollRectEntry<TKeyEntryData, TEntryData>> _unboundEntries;
            RectTransform _poolParent;
            RectTransform _endcapParent;

            _recycledEntries = GetRecyclerPrivateFieldValue<RecycledEntries<TKeyEntryData, TEntryData>>(nameof(_recycledEntries));
            _unboundEntries = GetRecyclerPrivateFieldValue<Queue<RecyclerScrollRectEntry<TKeyEntryData, TEntryData>>>(nameof(_unboundEntries));
            _poolParent = GetRecyclerPrivateFieldValue<RectTransform>(nameof(_poolParent));
            _endcapParent = GetRecyclerPrivateFieldValue<RectTransform>(nameof(_endcapParent));

            // Check that each inactive entry reports that it's waiting in the pool
            foreach (RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry in _recycledEntries.Entries.Values.Concat(
                         _unboundEntries))
            {
                if (entry.transform.parent != _poolParent)
                {
                    TestRecyclerEditorLogger.LogErrorAndBreak($"Entries not active in the recycler should be in the recycling pool. Entry \"{entry.Index}\" isn't.");
                    return;
                }
            }

            RecyclerScrollRectEndcap<TKeyEntryData, TEntryData> endcap = _recycler.Endcap;
            if (endcap != null && !endcap.gameObject.activeInHierarchy && endcap.transform.parent != _endcapParent)
            {
                TestRecyclerEditorLogger.LogErrorAndBreak($"An inactive endcap should be waiting in its recycling pool.");
            }
        }

        private TFieldValue GetRecyclerPrivateFieldValue<TFieldValue>(string fieldName)
        {
            return RecyclerScrollRectReflectionHelpers.GetPrivateFieldValue<TFieldValue, TEntryData, TKeyEntryData>(_recycler, fieldName);
        }
    }
}
