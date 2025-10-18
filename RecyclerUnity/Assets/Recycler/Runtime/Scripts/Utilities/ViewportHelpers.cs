using UnityEngine;

namespace Swill.Recycler
{
    /// <summary>
    /// Helpful functions when dealing with viewports.
    /// </summary>
    public static class ViewportHelpers
    {
        private static readonly Vector3[] RectCorners = new Vector3[4];
        private static readonly Vector3[] ViewportCorners = new Vector3[4];
        
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
            rectTransform.GetWorldCorners(RectCorners);
            viewport.GetWorldCorners(ViewportCorners);

            if (canvasCamera != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    RectCorners[i] = canvasCamera.WorldToScreenPoint(RectCorners[i]);
                    ViewportCorners[i] = canvasCamera.WorldToScreenPoint(ViewportCorners[i]);
                }   
            }

            float viewportWidth = ViewportCorners[2].x - ViewportCorners[0].x;
            float viewportBufferWidth = viewportWidth * bufferViewportPct;

            float viewportHeight = ViewportCorners[2].y - ViewportCorners[0].y;
            float viewportBufferHeight = viewportHeight * bufferViewportPct;

            Rect rect = new Rect(RectCorners[0].x, RectCorners[0].y, RectCorners[2].x - RectCorners[0].x, RectCorners[2].y - RectCorners[0].y);
            Rect viewportRect = new Rect(
                ViewportCorners[0].x - viewportBufferWidth / 2f, 
                ViewportCorners[0].y - viewportBufferHeight / 2f, 
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
