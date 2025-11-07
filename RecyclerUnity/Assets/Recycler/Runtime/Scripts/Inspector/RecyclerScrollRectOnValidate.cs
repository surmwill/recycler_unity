#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Swill.Recycler
{
    /// <summary>
    /// Contains the editor OnValidate call for the RecyclerScrollRect.
    /// Used for automatically setting up the GameObject structure when adding the component.
    /// </summary>
    public partial class RecyclerScrollRect<TKeyEntryData, TEntryData>
    {
        private const string ContentName = "Entries";
        private const string PoolParentName = "Pool";
        private const string EndcapParentName = "Endcap";
        
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

            // Proper scroll direction
            (horizontal, vertical) = (Orientation.IsHorizontal(), Orientation.IsVertical());

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
                
                // Default have the entries under their own canvas as they're constantly moving and dirtying themselves, but the user can change this
                content.gameObject.AddComponent<Canvas>();
                content.gameObject.AddComponent<GraphicRaycaster>();
            }
            
            // Set up the entries list for the orientation
            if (Orientation.IsVertical() && content.GetComponent<HorizontalLayoutGroup>() != null)
            {
                EditorUtils.OnValidateDestroy(content.GetComponent<HorizontalLayoutGroup>(), ValidateLayoutGroups);
            }
            else if (Orientation.IsHorizontal() && content.GetComponent<VerticalLayoutGroup>() != null)
            {
                EditorUtils.OnValidateDestroy(content.GetComponent<VerticalLayoutGroup>(), ValidateLayoutGroups);
            }
            else
            {
                ValidateLayoutGroups();
            }

            // Create a default pool
            if (_poolParent == null)
            {
                _poolParent = RectTransformFactory.CreateFullRect(PoolParentName, transform);
            }

            // Remove old entries from the pool that are not the current entry prefab (for example, we changed prefabs)
            foreach (RecyclerScrollRectEntry<TKeyEntryData, TEntryData> oldEntry in _poolParent
                         .GetComponentsInChildren<RecyclerScrollRectEntry<TKeyEntryData, TEntryData>>(true)
                         .Where(e => _recyclerEntryPrefab == null || !IsInstanceOfEntryPrefab(e)))
            {
                EditorUtils.OnValidateDestroy(oldEntry.gameObject);
            }

            // Ensure the pool is the correct size
            if (_recyclerEntryPrefab != null)
            {
                RecyclerScrollRectEntry<TKeyEntryData, TEntryData>[] currentEntries = _poolParent
                    .GetComponentsInChildren<RecyclerScrollRectEntry<TKeyEntryData, TEntryData>>(true)
                    .Where(IsInstanceOfEntryPrefab)
                    .ToArray();

                int poolDifference = _poolSize - currentEntries.Length;

                // Add any missing entries
                for (int i = 0; i < poolDifference; i++)
                {
                    RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry = ((GameObject) PrefabUtility.InstantiatePrefab(_recyclerEntryPrefab.gameObject, _poolParent))
                        .GetComponent<RecyclerScrollRectEntry<TKeyEntryData, TEntryData>>();

                    entry.name = RecyclerScrollRectEntry<TKeyEntryData, TEntryData>.UnboundIndex.ToString();
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
                    _endcap = _endcapParent.GetComponentsInChildren<RecyclerScrollRectEndcap<TKeyEntryData, TEntryData>>(true).FirstOrDefault(IsInstanceOfEndcapPrefab);

                    if (_endcap == null)
                    {
                        _endcap = ((GameObject) PrefabUtility.InstantiatePrefab(_endcapPrefab.gameObject, _endcapParent)).GetComponent<RecyclerScrollRectEndcap<TKeyEntryData, TEntryData>>();
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

        private void ValidateLayoutGroups()
        {
            // Ensure we have the proper layout group present
            if (Orientation.IsVertical() && content.GetComponent<VerticalLayoutGroup>() == null)
            {
                VerticalLayoutGroup v = content.gameObject.AddComponent<VerticalLayoutGroup>();
                (v.childControlWidth, v.childControlHeight) = (false, false);
                (v.childForceExpandWidth, v.childForceExpandHeight) = (false, false);
            }
            else if (Orientation.IsHorizontal() && content.GetComponent<HorizontalLayoutGroup>() == null)
            {
                HorizontalLayoutGroup h = content.gameObject.AddComponent<HorizontalLayoutGroup>();
                (h.childControlWidth, h.childControlHeight) = (false, false);
                (h.childForceExpandWidth, h.childForceExpandHeight) = (false, false);
            } 
            
            // Ensure the entries' root is not controlling the entries' widths or heights
            HorizontalOrVerticalLayoutGroup layoutGroup = content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layoutGroup.childControlWidth || layoutGroup.childControlHeight || layoutGroup.childForceExpandWidth || layoutGroup.childControlHeight)
            {
                Debug.LogWarning(
                    $"The {nameof(HorizontalOrVerticalLayoutGroup)} on the entries' root cannot control the entries' dimensions, it only positions them. Setting appropriately.\n" +
                    $"Entries can still be auto-sized using their own {nameof(HorizontalOrVerticalLayoutGroup)} and {nameof(ContentSizeFitter)}.\n" +
                    $"See Documentation or Samples for more.");

                (layoutGroup.childControlWidth, layoutGroup.childControlHeight) = (false, false);
                (layoutGroup.childForceExpandWidth, layoutGroup.childForceExpandHeight) = (false, false);
            }

            // Ensure the content resizes along with the total size of the entries
            ContentSizeFitter csf = content.GetComponent<ContentSizeFitter>();
            if (csf == null)
            {
                csf = content.gameObject.AddComponent<ContentSizeFitter>();
                if (Orientation.IsVertical())
                {
                    csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                }
                else
                {
                    csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                    csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;  
                }
            }

            bool csfHasWrongValues = false;
            
            if (Orientation.IsVertical() && (csf.verticalFit != ContentSizeFitter.FitMode.PreferredSize || csf.horizontalFit != ContentSizeFitter.FitMode.Unconstrained))
            {
                csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                csfHasWrongValues = true;
            } 
            else if (Orientation.IsHorizontal() && (csf.horizontalFit != ContentSizeFitter.FitMode.PreferredSize || csf.verticalFit != ContentSizeFitter.FitMode.Unconstrained))
            {
                csf.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
                csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                csfHasWrongValues = true;  
            }

            if (csfHasWrongValues)
            {
                Debug.LogWarning($"The {nameof(ContentSizeFitter)} on the entries' root must have a vertical fit of `{csf.verticalFit}` " +
                                 $"and horizontal fit `{csf.horizontalFit}` to match the orientation of the recycler. Setting appropriately.");
            }
        }
        
        private bool IsInstanceOfEntryPrefab(RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry)
        {
            return IsInstanceOfPrefab(entry, _recyclerEntryPrefab);
        }

        private bool IsInstanceOfEndcapPrefab(RecyclerScrollRectEndcap<TKeyEntryData, TEntryData> endcap)
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