using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Contains editor and debugging calls for our recycler scroll rect
/// </summary>
public partial class RecyclerScrollRect<TEntryData>
{
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

        // Ensure there is content
        if (content == null)
        {
            RectTransform entriesParent = (RectTransform) new GameObject("Entries", 
                typeof(RectTransform),
                typeof(VerticalLayoutGroup), typeof(ContentSizeFitter),
                typeof(Canvas), typeof(GraphicRaycaster)).transform;
            
            entriesParent.SetParent(transform);

            // Entries are in charge of their own width and height
            VerticalLayoutGroup v = entriesParent.GetComponent<VerticalLayoutGroup>();
            (v.childForceExpandWidth, v.childForceExpandHeight) = (false, false);

            // Grow the list along with the entries
            ContentSizeFitter c = entriesParent.GetComponent<ContentSizeFitter>();
            c.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            content = entriesParent;
        }
        
        // Set anchors and pivot according to configuration (top-down or bottom-up)
        content.pivot = content.pivot.WithY(AreEntriesIncreasing ? 1 : 0);
        content.anchorMin = new Vector2(0f, AreEntriesIncreasing ? 1 : 0);
        content.anchorMax = new Vector2(1f, AreEntriesIncreasing ? 1 : 0);
        content.anchoredPosition = Vector2.zero;
        (content.offsetMin, content.offsetMax) = (Vector2.zero, Vector2.zero);

        // Ensure there is a pool
        if (_poolParent == null)
        {
            _poolParent = RectTransformFactory.CreateFullRect("Pool", content.parent);
        }

        // Ensure the pool is the correct size
        if (_recyclerEntryPrefab != null)
        {
            int numMissingInPool = _poolSize - _poolParent.childCount;

            for (int i = 0; i < numMissingInPool; i++)
            {
                RecyclerScrollRectEntry<TEntryData> entry = (RecyclerScrollRectEntry<TEntryData>) PrefabUtility.InstantiatePrefab(_recyclerEntryPrefab, _poolParent);
                entry.name = RecyclerScrollRectEntry<TEntryData>.UnboundIndex.ToString();
                entry.gameObject.SetActive(false);
            }
            
            for (int i = 0; i < numMissingInPool * -1; i++)
            {
                EditorUtils.DestroyOnValidate(_poolParent.GetChild(i).gameObject);
            }
        }
        
        // Ensure we have a single end-cap pooled if one is provided
        if (_endcapPrefab != null)
        {
            if (_endcapParent == null)
            {
                _endcapParent = RectTransformFactory.CreateFullRect("Endcap", content.parent);
            }

            if (_endcapParent.childCount != 1)
            {
                for (int i = _endcapParent.childCount - 1; i >= 0; i--)
                {
                    EditorUtils.DestroyOnValidate(_endcapParent.GetChild(i).gameObject);
                }
                ((RecyclerEndcap<TEntryData>) PrefabUtility.InstantiatePrefab(_endcapPrefab, _endcapParent)).gameObject.SetActive(false);
            }   
        }
        else if (_endcapParent != null)
        {
            EditorUtils.DestroyOnValidate(_endcapParent.gameObject);
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
