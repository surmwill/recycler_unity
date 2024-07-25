#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Contains editor and debugging calls for our recycler scroll rect
    /// </summary>
    public partial class RecyclerScrollRect<TEntryData, TKeyEntryData>
    {
        private const string ContentName = "Entries";
        private const string PoolParentName = "Pool";
        private const string EndcapParentName = "Endcap";

        private DrivenRectTransformTracker _tracker;

        private RecyclerPosition _lastAppendTo = DefaultAppendTo == RecyclerPosition.Bot ? RecyclerPosition.Top : RecyclerPosition.Bot;

        private RectTransform _lastContent;

        protected override void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }

            _numCachedAtEachEnd = Mathf.Max(1, _numCachedAtEachEnd);
            _poolSize = Mathf.Max(0, _poolSize);

            // Vertical only (for now)
            if (!vertical || horizontal)
            {
                Debug.LogWarning("Only vertical RecyclerScrollRects are currently supported.");
                (vertical, horizontal) = (true, false);
            }

            // Clamped only
            if (movementType != MovementType.Clamped)
            {
                Debug.LogWarning("Only clamped movement is supported.");
                movementType = MovementType.Clamped;
            }

            // Create a default viewport
            if (viewport == null)
            {
                viewport = (RectTransform) transform;
            }

            // Create default content (the root of the list of entries)
            if (content == null)
            {
                RectTransform entriesParent = (RectTransform) new GameObject(ContentName,
                    typeof(RectTransform),
                    typeof(VerticalLayoutGroup), typeof(ContentSizeFitter),
                    typeof(Canvas), typeof(GraphicRaycaster)).transform;

                entriesParent.SetParent(transform);
                content = entriesParent;
                (content.localPosition, content.localRotation, content.localScale) = (Vector3.zero, Quaternion.identity, Vector3.one);
                (content.offsetMin, content.offsetMax) = (Vector2.zero, Vector2.zero);
            }

            // When appending downwards by default we start at the top, and vice-versa 
            if (_lastAppendTo != _appendTo)
            {
                content.pivot = content.pivot.WithY(_appendTo == RecyclerPosition.Bot ? 1 : 0);
                _lastAppendTo = _appendTo;
            }

            // Create a default pool
            if (_poolParent == null)
            {
                _poolParent = RectTransformFactory.CreateFullRect(PoolParentName, transform);
            }

            // Remove old entries from the pool that are not the current entry prefab (for example, we changed prefabs)
            foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> oldEntry in _poolParent
                         .GetComponentsInChildren<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>(true)
                         .Where(e => _recyclerEntryPrefab == null || !IsInstanceOfEntryPrefab(e)))
            {
                EditorUtils.OnValidateDestroy(oldEntry.gameObject);
            }

            // Ensure the pool is the correct size
            if (_recyclerEntryPrefab != null)
            {
                RecyclerScrollRectEntry<TEntryData, TKeyEntryData>[] currentEntries = _poolParent
                    .GetComponentsInChildren<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>(true)
                    .Where(IsInstanceOfEntryPrefab)
                    .ToArray();

                int poolDifference = _poolSize - currentEntries.Length;

                // Add any missing entries
                for (int i = 0; i < poolDifference; i++)
                {
                    RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry = ((GameObject) PrefabUtility.InstantiatePrefab(_recyclerEntryPrefab.gameObject, _poolParent))
                        .GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();

                    entry.name = RecyclerScrollRectEntry<TEntryData, TKeyEntryData>.UnboundIndex.ToString();
                    entry.gameObject.SetActive(false);
                }

                // Delete any extra entries
                if (poolDifference < 0)
                {
                    for (int i = 0; i < Mathf.Min(currentEntries.Length, Mathf.Abs(poolDifference)); i++)
                    {
                        EditorUtils.OnValidateDestroy(currentEntries[i].gameObject);
                    }
                }
            }

            // Ensure we have a single end-cap pooled if one is provided
            if (_endcapPrefab != null)
            {
                // If we have an old endcap, get rid of it
                if (_endcap != null && !IsInstanceOfEndcapPrefab(_endcap))
                {
                    EditorUtils.OnValidateDestroy(_endcap.gameObject);
                    _endcap = null;
                }

                // Ensure there is a pool for the endcap
                if (_endcapParent == null)
                {
                    _endcapParent = RectTransformFactory.CreateFullRect(EndcapParentName, transform);
                }

                // Ensure the endcap exists in the pool
                if (_endcap == null)
                {
                    _endcap = _endcapParent.GetComponentsInChildren<RecyclerScrollRectEndcap<TEntryData, TKeyEntryData>>(true)
                        .FirstOrDefault(IsInstanceOfEndcapPrefab);

                    if (_endcap == null)
                    {
                        _endcap = ((GameObject) PrefabUtility.InstantiatePrefab(_endcapPrefab.gameObject, _endcapParent))
                            .GetComponent<RecyclerScrollRectEndcap<TEntryData, TKeyEntryData>>();
                        
                        _endcap.gameObject.SetActive(false);
                    }
                }
            }
            // The prefab is null, if reference to the endcap is not, then destroy the endcap (we must be swapping out endcaps) 
            else if (_endcap != null)
            {
                EditorUtils.OnValidateDestroy(_endcap.gameObject);
            }
        }

        /// <summary>
        /// Ensure we have a collider with the correct values to detect when things are on/offscreen
        /// </summary>
        private void EditorSetViewportColliderDimensions()
        {
            if (viewport == null)
            {
                return;
            }

            BoxCollider bc = viewport.GetComponent<BoxCollider>();
            if (bc == null)
            {
                bc = viewport.gameObject.AddComponent<BoxCollider>();
            }

            bc.size = new Vector3(viewport.rect.width, viewport.rect.height, 1f);
        }

        /// <summary>
        /// Ensure the root of all the entries has the necessary components with the necessary fields checked
        /// </summary>
        private void EditorCheckRootEntriesComponents()
        {
            if (content == null)
            {
                return;
            }

            // Ensure the root of all entries has the proper anchor values.
            // Importantly, the anchored position will treated differently if the y's don't match (though the value we choose doesn't matter)
            if (content != _lastContent)
            {
                _tracker.Clear();
                _lastContent = content;
            }

            //_tracker.Add(this, content, DrivenTransformProperties.AnchorMin | DrivenTransformProperties.AnchorMax);
            content.anchorMin = new Vector2(0f, 0.5f);
            content.anchorMax = new Vector2(1f, 0.5f);

            // Ensure the entries' root is not controlling the entries' widths or heights
            VerticalLayoutGroup v = content.GetComponent<VerticalLayoutGroup>();
            if (v == null)
            {
                v = content.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            if (v.childControlWidth || v.childControlHeight)
            {
                Debug.LogWarning(
                    $"The {nameof(VerticalLayoutGroup)} on the entries' root cannot have {nameof(v.childControlWidth)} or {nameof(v.childControlHeight)} checked for performance reasons; unchecking them.\n" +
                    $"Entries can still be auto-sized by controlling their own width and height with their own {nameof(ContentSizeFitter)}.\n" +
                    $"Please see documentation for more.");

                (v.childControlWidth, v.childControlHeight) = (false, false);
            }

            (v.childForceExpandWidth, v.childForceExpandHeight) = (false, false);

            // Ensure the content resizes along with the total size of the entries
            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = content.gameObject.AddComponent<ContentSizeFitter>();
            }

            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        /// <summary>
        /// Check that the indices we report as visible, in the start cache, and in the end cache, correspond to actual
        /// entries that are visible, in the start cache, and in the end cache
        /// </summary>
        private void DebugCheckWindow()
        {
            HashSet<int> indicesInStartCache = new HashSet<int>();
            HashSet<int> indicesInEndCache = new HashSet<int>();
            HashSet<int> visibleIndices = new HashSet<int>();

            // Check which indices we report as visible, in the start cache, and in the end cache
            if (_activeEntriesWindow.StartCacheIndexRange.HasValue)
            {
                (int Start, int End) = _activeEntriesWindow.StartCacheIndexRange.Value;
                indicesInStartCache = new HashSet<int>(Enumerable.Range(Start, End - Start + 1));
            }

            if (_activeEntriesWindow.EndCacheIndexRange.HasValue)
            {
                (int Start, int End) = _activeEntriesWindow.EndCacheIndexRange.Value;
                indicesInEndCache = new HashSet<int>(Enumerable.Range(Start, End - Start + 1));
            }

            if (_activeEntriesWindow.VisibleIndexRange.HasValue)
            {
                (int Start, int End) = _activeEntriesWindow.VisibleIndexRange.Value;
                visibleIndices = new HashSet<int>(Enumerable.Range(Start, End - Start + 1));
            }

            foreach (Transform t in content)
            {
                RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry = t.GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();
                if (entry == null)
                {
                    return;
                }

                // Entries that are visible in the viewport should be reported as visible
                if (IsInViewport(entry.RectTransform))
                {
                    if (!visibleIndices.Remove(entry.Index))
                    {
                        Debug.LogError($"{entry.Index} should be in the visible index window.\n\n {_activeEntriesWindow.PrintRanges()}");
                        Debug.Break();
                        return;   
                    }
                }
                // Entries that are above the viewport should be reported as in the start/end cache, depending on orientation
                else if (IsAboveViewport(entry.RectTransform))
                {
                    if (StartCachePosition == RecyclerPosition.Top)
                    {
                        if (!indicesInStartCache.Remove(entry.Index))
                        {
                            Debug.LogError($"{entry.Index} should be in the start cache window.\n\n {_activeEntriesWindow.PrintRanges()}");
                            Debug.Break();
                            return;   
                        }
                    }
                    else if (EndCachePosition == RecyclerPosition.Top)
                    {
                        if (!indicesInEndCache.Remove(entry.Index))
                        {
                            Debug.LogError($"{entry.Index} should be in the end cache window.\n\n {_activeEntriesWindow.PrintRanges()}");
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
                            Debug.LogError($"{entry.Index} should be in the start cache window.\n\n {_activeEntriesWindow.PrintRanges()}");
                            Debug.Break();
                            return;   
                        }
                    }
                    
                    if (EndCachePosition == RecyclerPosition.Bot)
                    {
                        if (!indicesInEndCache.Remove(entry.Index))
                        {
                            Debug.LogError($"{entry.Index} should be in the end cache window.\n\n {_activeEntriesWindow.PrintRanges()}");
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
            foreach (Transform t in content)
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
            foreach (Transform t in content)
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
            for (int i = 0; i < _dataForEntries.Count; i++)
            {
                TKeyEntryData actualKey = _dataForEntries[i].Key;
                TKeyEntryData mappedKey = GetKeyForCurrentIndex(i);

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
            for (int i = 0; i < _dataForEntries.Count; i++)
            {
                TKeyEntryData key = _dataForEntries[i].Key;
                int mappedIndex = GetCurrentIndexForKey(_dataForEntries[i].Key);

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
            // No indices reported and no references to active entries
            if (!ActiveEntriesWindow.ActiveEntriesRange.HasValue && !ActiveEntries.Any())
            {
                return;
            }

            // No indices reported but references to active entries
            if (!ActiveEntriesWindow.ActiveEntriesRange.HasValue && ActiveEntries.Any())
            {
                Debug.LogError("The window states there are no active indices, but we are still referencing active entries.");
                Debug.Break();
                return;
            }

            (int activeIndicesStart, int activeIndicesEnd) = ActiveEntriesWindow.ActiveEntriesRange.Value;

            // Check that each active index has a corresponding reference to an active entry
            for (int i = activeIndicesStart; i <= activeIndicesEnd; i++)
            {
                if (!ActiveEntries.ContainsKey(i))
                {
                    Debug.LogError($"The window states that index {i} should be active, but there is no reference to an active entry with that index.");
                    Debug.Break();
                    return;
                }
            }

            // Check that each reference to an active entry has a corresponding active index
            foreach (int index in ActiveEntries.Keys)
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
            // Check that each active entry's state reflect its actual position in the list
            foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in ActiveEntries.Values)
            {
                switch (entry.State)
                {
                    // Visible
                    case RecyclerScrollRectContentState.ActiveVisible:
                    {
                        if (!IsInViewport(entry.RectTransform))
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
                        if ((StartCachePosition == RecyclerPosition.Top && !IsAboveViewport(entry.RectTransform)) ||
                             (StartCachePosition == RecyclerPosition.Bot && !IsBelowViewport(entry.RectTransform)))
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
                        if ((EndCachePosition == RecyclerPosition.Top && !IsAboveViewport(entry.RectTransform)) ||
                            (EndCachePosition == RecyclerPosition.Bot && !IsBelowViewport(entry.RectTransform)))
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
            foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in ActiveEntries.Values.Concat(_recycledEntries.Entries.Values).Concat(_unboundEntries))
            {
                RecyclerScrollRectContentState recyclerReportedEntryState = GetStateOfEntryWithIndex(entry.Index);
                if (recyclerReportedEntryState != entry.State)
                {
                    Debug.LogError($"Mismatch between the state contained in entry {entry.Index} \"{entry.State}\" and the recycler's view on its state \"{recyclerReportedEntryState}\".");
                    Debug.Break();
                    return;
                }   
            }

            if (_endcap == null)
            {
                return;
            }

            // Check that the endcap's state reflects its actual position in the list
            switch (_endcap.State)
            {
                // Visible
                case RecyclerScrollRectContentState.ActiveVisible:
                {
                    if (!IsInViewport(_endcap.RectTransform))
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
                    if (EndCachePosition == RecyclerPosition.Top && !IsAboveViewport(_endcap.RectTransform) || 
                        EndCachePosition == RecyclerPosition.Top && !IsBelowViewport(_endcap.RectTransform))
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
                    if (_endcap.gameObject.activeSelf)
                    {
                        Debug.LogError("The endcap should not be active while in the pool");
                        Debug.Break();
                        return;
                    }
                    break;
                }
            }
            
            // Check that the state of the endcap reflects its actual position in the list
            if (_endcap.State == RecyclerScrollRectContentState.ActiveVisible && !IsInViewport(_endcap.RectTransform))
            {
                Debug.LogError("The endcap's state says it's visible but its position in the list does not reflect this.");
                Debug.Break();
                return;
            }
        }

        private bool IsInstanceOfEntryPrefab(RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry)
        {
            return IsInstanceOfPrefab(entry, _recyclerEntryPrefab);
        }

        private bool IsInstanceOfEndcapPrefab(RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> endcap)
        {
            return IsInstanceOfPrefab(endcap, _endcapPrefab);
        }

        private bool IsInstanceOfPrefab(Object instanceComponentOrGameObject, Object prefabAsset)
        {
            return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceComponentOrGameObject) ==
                   AssetDatabase.GetAssetPath(prefabAsset);
        }
    }
}

#endif