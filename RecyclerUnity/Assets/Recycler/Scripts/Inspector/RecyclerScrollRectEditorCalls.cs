#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Contains editor calls for the RecyclerScrollRect.
    /// </summary>
    public partial class RecyclerScrollRect<TEntryData, TKeyEntryData>
    {
        private const string ContentName = "Entries";
        private const string PoolParentName = "Pool";
        private const string EndcapParentName = "Endcap";

        private RectTransform _lastContent;
        private (bool, bool)? _lastOrientation;
        private MovementType? _lastMovementType;

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
                if (_lastOrientation.HasValue)
                {
                    Debug.LogWarning("Only vertical RecyclerScrollRects are currently supported. Setting appropriately.");   
                }
                
                (vertical, horizontal) = (true, false);
                _lastOrientation = (vertical, horizontal);
            }

            // Clamped only
            if (movementType != MovementType.Clamped)
            {
                if (_lastMovementType.HasValue)
                {
                    Debug.LogWarning("Only clamped movement is supported. Setting appropriately.");   
                }
                
                movementType = MovementType.Clamped;
                _lastMovementType = movementType;
            }

            // Create a default viewport
            if (viewport == null)
            {
                viewport = (RectTransform) transform;
                
                // A RectMask will only render things in the viewport, increasing performance, but the user can change this
                viewport.gameObject.AddComponent<RectMask2D>();
            }

            // Create default content (the root of the list of entries)
            if (content == null)
            {
                RectTransform entriesParent = (RectTransform) new GameObject(ContentName, typeof(RectTransform)).transform;
                entriesParent.SetParent(transform);
                content = entriesParent;
                
                (content.localPosition, content.localRotation, content.localScale) = (Vector3.zero, Quaternion.identity, Vector3.one);
                (content.offsetMin, content.offsetMax) = (Vector2.zero, Vector2.zero);
                
                InspectorCheckRootEntriesComponents();
                
                // Default have the entries under their own canvas as they're constantly moving and dirtying themselves, but the user can change this
                content.gameObject.AddComponent<Canvas>();
                content.gameObject.AddComponent<GraphicRaycaster>();
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
        /// Ensure the root of all the entries has the necessary components with the necessary fields checked
        /// </summary>
        private void InspectorCheckRootEntriesComponents()
        {
            if (content == null)
            {
                return;
            }

            // Ensure the root of all entries has the proper anchor values.
            // Importantly, the anchored position will treated differently if the y's don't match (although the value we choose isn't important)
            if (content != _lastContent)
            {
                _tracker.Clear();
                _lastContent = content;
            }
            SetContentTracker();
            
            // Ensure the entries' root is not controlling the entries' widths or heights
            VerticalLayoutGroup v = content.GetComponent<VerticalLayoutGroup>();
            if (v == null)
            {
                v = content.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            if (v.childControlWidth || v.childControlHeight)
            {
                Debug.LogWarning(
                    $"The {nameof(VerticalLayoutGroup)} on the entries' root cannot have {nameof(v.childControlWidth)} or {nameof(v.childControlHeight)} checked for performance reasons. Setting appropriately.\n" +
                    $"Entries can still be auto-sized by controlling their own width and height through their own {nameof(ContentSizeFitter)}.\n" +
                    $"See Documentation for more.");

                (v.childControlWidth, v.childControlHeight) = (false, false);
            }

            // Ensure the content resizes along with the total size of the entries
            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = content.gameObject.AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (csf.verticalFit != ContentSizeFitter.FitMode.PreferredSize)
            {
                Debug.LogWarning($"The {nameof(ContentSizeFitter)} on the entries' root must have a vertical fit of {nameof(ContentSizeFitter.FitMode.PreferredSize)} to match the size of the list of entries. Setting appropriately.");
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;   
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