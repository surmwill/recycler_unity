using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>
/// Contains editor and debugging calls for our recycler scroll rect
/// </summary>
public partial class RecyclerScrollRect<TEntryData, TKeyEntryData>
{
    private const string ContentName = "Entries";
    private const string PoolParentName = "Pool";
    private const string EndcapParentName = "Endcap";
    
    protected override void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }
        
        _numCachedBeforeStart = Mathf.Max(1, _numCachedBeforeStart);
        _numCachedAfterEnd = Mathf.Max(1, _numCachedAfterEnd);
        _poolSize = Mathf.Max(0, _poolSize);

        // Vertical only supported (currently)
        (vertical, horizontal) = (true, false);

        // Clamped only
        movementType = MovementType.Clamped;
        
        // Ensure there is a viewport
        if (viewport == null)
        {
            viewport = (RectTransform) transform;
        }
        
        // Ensure the viewport has a collider to test what is visible
        InitViewportCollider();

        // Ensure there is content (the active list of entries)
        if (content == null)
        {
            RectTransform entriesParent = (RectTransform) new GameObject(ContentName, 
                typeof(RectTransform),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter),
                typeof(Canvas), typeof(GraphicRaycaster)).transform;
            
            entriesParent.SetParent(transform);
            content = entriesParent;

            // Grow the list along with the entries
            ContentSizeFitter c = entriesParent.GetComponent<ContentSizeFitter>();
            c.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Default assume we are force expanding each entries width to fit the width of the Recycler
            VerticalLayoutGroup v = entriesParent.GetComponent<VerticalLayoutGroup>();
            (v.childForceExpandWidth, v.childForceExpandHeight) = (false, false);

            // Entries will start at the top if we're appending downwards, or the bottom if we're appending upwards
            (content.localPosition, content.localScale) = (Vector3.zero, Vector3.one);
            (content.anchorMin, content.anchorMax) = (new Vector2(0f, AreEntriesIncreasing ? 1 : 0), new Vector2(1f, AreEntriesIncreasing ? 1 : 0));
            (content.offsetMin, content.offsetMax) = (Vector2.zero, Vector2.zero);
            content.anchoredPosition = Vector2.zero;

            // Appended entries will grow downwards (not pushing any higher entries) when we're appending downwards,
            // or grow upwards (not pushing any lower entries) when we're appending upwards.
            content.pivot = content.pivot.WithY(AreEntriesIncreasing ? 1 : 0);
        }

        // Ensure there is a pool of waiting to be bound entries
        if (_poolParent == null)
        {
            _poolParent = RectTransformFactory.CreateFullRect(PoolParentName, transform);
        }
        
        // Remove old entries from the pool that are not the current entry prefab (for example, we changed prefabs)
        foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> oldEntry in _poolParent.GetComponentsInChildren<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>(true)
                     .Where(e => _recyclerEntryPrefab == null || !IsInstanceOfEntryPrefab(e)))
        {
            EditorUtils.DestroyOnValidate(oldEntry.gameObject);
        }

        // Ensure the pool is the correct size
        if (_recyclerEntryPrefab != null)
        {
            RecyclerScrollRectEntry<TEntryData, TKeyEntryData>[] currentEntries = _poolParent.GetComponentsInChildren<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>(true)
                .Where(IsInstanceOfEntryPrefab)
                .ToArray();
            
            int poolDifference = _poolSize - currentEntries.Length;

            // Add any missing entries
            for (int i = 0; i < poolDifference; i++)
            {
                RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry =
                    ((GameObject) PrefabUtility.InstantiatePrefab(_recyclerEntryPrefab.gameObject, _poolParent))
                        .GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();

                entry.name = RecyclerScrollRectEntry<TEntryData, TKeyEntryData>.UnboundIndex.ToString();
                entry.gameObject.SetActive(false);
            }

            // Delete any extra entries
            if (poolDifference < 0)
            {
                for (int i = 0; i < Mathf.Min(currentEntries.Length, Mathf.Abs(poolDifference)); i++)
                {
                    EditorUtils.DestroyOnValidate(currentEntries[i].gameObject);
                }
            }
        }
        
        // Ensure we have a single end-cap pooled if one is provided
        if (_endcapPrefab != null)
        {
            // If we have an old endcap, get rid of it
            if (_endcap != null && !IsInstanceOfEndcapPrefab(_endcap))
            {
                EditorUtils.DestroyOnValidate(_endcap.gameObject);
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
                _endcap = _endcapParent.GetComponentsInChildren<RecyclerScrollRectEndcap<TEntryData, TKeyEntryData>>(true).FirstOrDefault(IsInstanceOfEndcapPrefab);

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
            EditorUtils.DestroyOnValidate(_endcap.gameObject);
        }
    }
    
    private void DebugCheckWindow()
    {
        HashSet<int> indicesInStartCache = new HashSet<int>();
        HashSet<int> indicesInEndCache = new HashSet<int>();
        HashSet<int> visibleIndices = new HashSet<int>();
        
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
            if (!t.gameObject.activeInHierarchy)
            {
                continue;
            }
            
            RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry = t.GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();
            if (entry == null)
            {
                return;
            }

            if (IsInViewport(entry.RectTransform))
            {
                if (!visibleIndices.Remove(entry.Index))
                {
                    Debug.LogError($"{entry.Index} should be in the visible index window.\n\n {_activeEntriesWindow.PrintRanges()}");
                    Debug.Break();
                    return;
                }
            }
            else if (IsAboveViewport(entry.RectTransform))
            {
                if (StartCacheTransformPosition == RecyclerTransformPosition.Top)
                {
                    if (!indicesInStartCache.Remove(entry.Index))
                    {
                        Debug.LogError($"{entry.Index} should be in the start cache window.\n\n {_activeEntriesWindow.PrintRanges()}");
                        Debug.Break();
                        return;
                    }
                }
                else
                {
                    if (!indicesInEndCache.Remove(entry.Index))
                    {
                        Debug.LogError($"{entry.Index} should be in the end cache window.\n\n {_activeEntriesWindow.PrintRanges()}");
                        Debug.Break();
                        return;
                    }
                }
            }
            else
            {
                if (StartCacheTransformPosition == RecyclerTransformPosition.Bot)
                {
                    if (!indicesInStartCache.Remove(entry.Index))
                    {
                        Debug.LogError($"{entry.Index} should be in the start cache window.\n\n {_activeEntriesWindow.PrintRanges()}");
                        Debug.Break();
                        return;
                    }
                }
                else
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
        return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceComponentOrGameObject) == AssetDatabase.GetAssetPath(prefabAsset);
    }

    private void DebugCheckDuplicates()
    {
        HashSet<int> seenIndices = new HashSet<int>();
        foreach (Transform t in content)
        {
            if (!t.gameObject.activeInHierarchy)
            {
                continue;
            }
            
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

    private void DebugCheckOrdering()
    {
        int? lastIndex = null;
        foreach (Transform t in content)
        {
            if (!t.gameObject.activeInHierarchy)
            {
                continue;
            }

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
}
