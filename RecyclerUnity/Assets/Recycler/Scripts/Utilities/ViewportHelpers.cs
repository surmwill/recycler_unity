using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Helpful functions when dealing with viewports.
    /// </summary>
    public static class ViewportHelpers
    {
        /// <summary>
        /// Returns true if a given RectTransform is contained in a viewport.
        /// </summary>
        /// <param name="rectTransform"> The RectTransform. </param>
        /// <param name="viewport"> The RectTransform of the viewport. </param>
        /// <param name="canvasCamera"> The camera attached to the canvas containing the RectTransform's (null for overlay canvases). </param>
        /// <param name="bufferViewportPct"> A buffer for the viewport: extends its width and height by this percentage. </param>
        /// <returns> True if the given RectTransform overlaps some part of the viewport </returns>
        public static bool IsInViewport(RectTransform rectTransform, RectTransform viewport, Camera canvasCamera, float bufferViewportPct = 0.001f)
        {
            Vector3[] rectCorners = new Vector3[4];
            Vector3[] viewportCorners = new Vector3[4];

            rectTransform.GetWorldCorners(rectCorners);
            viewport.GetWorldCorners(viewportCorners);

            if (canvasCamera != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    rectCorners[i] = canvasCamera.WorldToScreenPoint(rectCorners[i]);
                    viewportCorners[i] = canvasCamera.WorldToScreenPoint(viewportCorners[i]);
                }   
            }

            float viewportWidth = viewportCorners[2].x - viewportCorners[0].x;
            float viewportBufferWidth = viewportWidth * bufferViewportPct;

            float viewportHeight = viewportCorners[2].y - viewportCorners[0].y;
            float viewportBufferHeight = viewportHeight * bufferViewportPct;

            Rect rect = new Rect(rectCorners[0].x, rectCorners[0].y, rectCorners[2].x - rectCorners[0].x, rectCorners[2].y - rectCorners[0].y);
            Rect viewportRect = new Rect(
                viewportCorners[0].x - viewportBufferWidth / 2f, 
                viewportCorners[0].y - viewportBufferHeight / 2f, 
                viewportWidth + viewportBufferWidth, 
                viewportHeight + viewportBufferHeight);

            return rect.Overlaps(viewportRect);
        }

        /// <summary>
        /// Returns true if a given RectTransform is above the center of a viewport.
        /// </summary>
        /// <param name="rectTransform"> The RectTransform. </param>
        /// <param name="viewport"> The RectTransform of the viewport. </param>
        /// <returns> True if the given RectTransform is above the center of the viewport. </returns>
        public static bool IsAboveViewportCenter(RectTransform rectTransform, RectTransform viewport)
        {
            return Vector3.Dot(Vector3.ProjectOnPlane(rectTransform.position - viewport.GetWorldRect().Center, viewport.forward), viewport.up) > 0;
        }
        
        /// <summary>
        /// Returns true if a given RectTransform is below the center of a viewport.
        /// </summary>
        /// <param name="rectTransform"> The RectTransform. </param>
        /// <param name="viewport"> The RectTransform of the viewport. </param>
        /// <returns> True if the given RectTransform is below the center of the viewport. </returns>
        public static bool IsBelowViewportCenter(RectTransform rectTransform, RectTransform viewport)
        {
            return Vector3.Dot(Vector3.ProjectOnPlane(rectTransform.position - viewport.GetWorldRect().Center, viewport.forward), -viewport.up) > 0;
        }
    }
}
