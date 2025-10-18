using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Extensions for RectTransforms
    /// </summary>
    public static class RectTransformExtensions
    {
        /// <summary>
        /// Sets the pivot of a RectTransform without changing its position.
        /// Note that this behavior is observed while adjusting pivots in the inspector, but not during play mode.
        /// </summary>
        /// <param name="rectTransform"> The RectTransform to adjust the pivot of. </param>
        /// <param name="newPivot"> The new pivot of the RectTransform. </param>
        public static void SetPivotWithoutMoving(this RectTransform rectTransform, Vector2 newPivot)
        {
            (Vector2 anchorMin, Vector2 anchorMax) = (rectTransform.anchorMin, rectTransform.anchorMax);
            (rectTransform.anchorMin, rectTransform.anchorMax) = (Vector2.one * 0.5f, Vector2.one * 0.5f);
            
            Vector2 offset = (rectTransform.pivot - newPivot) * -1f;
            offset.Scale(rectTransform.rect.size);
            
            rectTransform.pivot = newPivot;
            rectTransform.anchoredPosition += offset;
            
            (rectTransform.anchorMin, rectTransform.anchorMax) = (anchorMin, anchorMax);
        }
        
        /// <summary>
        /// Returns a rectangle of the 4 world space corners that make up the RectTransform.
        /// Note that the built-in RectTransform.rect is defined locally and not relative to the world.
        /// </summary>
        /// <param name="rectTransform"> The RectTransform to get the world corners of. </param>
        /// <returns> A WorldRect containing the 4 world space corners of the RectTransform. </returns>
        public static WorldRect GetWorldRect(this RectTransform rectTransform)
        {
            return new WorldRect(rectTransform);
        }
    }
}
