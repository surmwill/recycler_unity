using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contains editor and debugging calls for our recycler scroll rect
/// </summary>
public partial class RecyclerScrollRect<TEntryData>
{
    private const string ContentName = "Entries";
    private const string PoolParentName = "Pool";
    private const string EndcapParentName = "Endcap";

    protected override void OnValidate()
    {
        _numCachedBeforeStart = Mathf.Max(1, _numCachedBeforeStart);
        _numCachedAfterEnd = Mathf.Max(1, _numCachedAfterEnd);
        _poolSize = Mathf.Max(0, _poolSize);

        // Vertical only supported (currently)
        (vertical, horizontal) = (true, false);
        
        // Ensure there is a viewport
        if (viewport == null)
        {
            viewport = (RectTransform) transform;
        }

        // Ensure there is content (the active list of entries)
        if (content == null)
        {
            RectTransform entriesParent = (RectTransform) new GameObject(ContentName, 
                typeof(RectTransform),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter),
                typeof(Canvas), typeof(GraphicRaycaster)).transform;
            
            entriesParent.SetParent(transform);
            content = entriesParent;

            // Entries are in charge of their own width and height
            VerticalLayoutGroup v = entriesParent.GetComponent<VerticalLayoutGroup>();
            (v.childForceExpandWidth, v.childForceExpandHeight) = (false, false);

            // Grow the list along with the entries
            ContentSizeFitter c = entriesParent.GetComponent<ContentSizeFitter>();
            c.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Entries will start at the top if we're appending downwards, or the bottom if we're appending upwards
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

        // Ensure the pool is the correct size
        if (_recyclerEntryPrefab != null)
        {
            int numInPool = _poolParent.Children().Count(t => t.HasComponent<RecyclerScrollRectEntry<TEntryData>>());
            int poolDifference = _poolSize - numInPool;

            // Add any missing entries
            for (int i = 0; i < poolDifference; i++)
            {
                RecyclerScrollRectEntry<TEntryData> entry = (RecyclerScrollRectEntry<TEntryData>) PrefabUtility.InstantiatePrefab(_recyclerEntryPrefab, _poolParent);
                entry.name = RecyclerScrollRectEntry<TEntryData>.UnboundIndex.ToString();
                entry.gameObject.SetActive(false);
            }

            // Delete any extra entries
            if (poolDifference < 0)
            {
                RecyclerScrollRectEntry<TEntryData>[] entries = _poolParent.GetComponentsInChildren<RecyclerScrollRectEntry<TEntryData>>(true);
                for (int i = 0; i < Mathf.Min(entries.Length, Mathf.Abs(poolDifference)); i++)
                {
                    EditorUtils.DestroyOnValidate(entries[i].gameObject);
                }
            }
        }
        
        // Ensure we have a single end-cap pooled if one is provided
        if (_endcapPrefab != null)
        {
            // Ensure there is a pool for the endcap
            if (_endcapParent == null)
            {
                _endcapParent = RectTransformFactory.CreateFullRect(EndcapParentName, transform);
            }

            // Ensure the endcap exists in the pool
            if (_endcap == null)
            {
                _endcap = _endcapParent.GetComponentInChildren<RecyclerEndcap<TEntryData>>(true);
                if (_endcap == null)
                {
                    _endcap = (RecyclerEndcap<TEntryData>) PrefabUtility.InstantiatePrefab(_endcapPrefab, _endcapParent);
                    _endcap.gameObject.SetActive(false);
                }
            }
        }
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
            
            RecyclerScrollRectEntry<TEntryData> entry = t.GetComponent<RecyclerScrollRectEntry<TEntryData>>();
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

            RecyclerScrollRectEntry<TEntryData> entry = t.GetComponent<RecyclerScrollRectEntry<TEntryData>>();
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
