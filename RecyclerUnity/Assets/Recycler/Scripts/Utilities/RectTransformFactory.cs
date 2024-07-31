using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Creator of commonly used RectTransforms.
    /// </summary>
    public static class RectTransformFactory
    {
        /// <summary>
        /// Creates a child RectTransform equal in size to its parent.
        /// </summary>
        /// <param name="name"> The name of the RectTransform. </param>
        /// <param name="parent"> The parent RectTransform. </param>
        /// <returns> A child RectTransform equal in size to its parent. </returns>
        public static RectTransform CreateFullRect(string name, Transform parent)
        {
            RectTransform rect = (RectTransform) new GameObject(name, typeof(RectTransform)).transform;

            rect.SetParent(parent);
            (rect.localScale, rect.localPosition, rect.localRotation) = (Vector3.one, Vector3.zero, Quaternion.identity);

            (rect.anchorMin, rect.anchorMax) = (Vector2.zero, Vector2.one);
            (rect.offsetMin, rect.offsetMax) = (Vector2.zero, Vector2.zero);

            return rect;
        }
    }
}
