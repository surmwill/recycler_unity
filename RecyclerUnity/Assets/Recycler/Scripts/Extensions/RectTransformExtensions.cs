using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Extensions for RectTransforms
    /// </summary>
    public static class RectTransformExtensions
    {
        /// <summary>
        /// Set pivot without changing the position of the element
        /// </summary>
        public static void SetPivotWithoutMoving(this RectTransform rectTransform, Vector2 newPivot)
        {
            (Vector2 anchorMin, Vector2 anchorMax) = (rectTransform.anchorMin, rectTransform.anchorMax);
            (rectTransform.anchorMin, rectTransform.anchorMax) = (Vector2.one * 0.5f, Vector2.one * 0.5f);
            
            Vector2 offset = (rectTransform.pivot - newPivot) * -1f;
            offset.Scale(rectTransform.rect.size);

            //Vector3 localPos = rectTransform.localPosition + offset.WithZ(0f);
            rectTransform.pivot = newPivot;
            rectTransform.anchoredPosition += offset;
            
            (rectTransform.anchorMin, rectTransform.anchorMax) = (anchorMin, anchorMax);
            //rectTransform.localPosition = localPos;
        }

        /// <summary>
        /// Returns a rectangle of the 4 world vertices that make up the RectTransform
        /// (Note the built in RectTransform.rect is defined locally and not relative to the world)
        /// </summary>
        public static WorldRect GetWorldRect(this RectTransform rectTransform)
        {
            return new WorldRect(rectTransform);
        }
    }
}
