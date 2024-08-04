using System.Linq;
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
        /// (Assumes the RectTransform and viewport are axis aligned.) 
        /// </summary>
        /// <param name="rectTransform"> The RectTransform. </param>
        /// <param name="viewportCollider"> A collider representing the viewport. </param>
        /// <returns> True if the given RectTransform is contained in the viewport. </returns>
        /*
        public static bool IsInViewport(RectTransform rectTransform, BoxCollider viewportCollider)
        {
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);
            return worldCorners.Any(viewportCollider.ContainsPoint);
        }
        */
        
        /// <summary>
        /// TODO: add percentage buffer
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <param name="viewport"></param>
        /// <returns></returns>
        public static bool IsInViewport(RectTransform rectTransform, RectTransform viewport)
        {
            Vector3[] rectCorners = new Vector3[4];
            Vector3[] viewportCorners = new Vector3[4];
            
            rectTransform.GetWorldCorners(rectCorners);
            viewport.GetWorldCorners(viewportCorners);
            
            Rect rect = new Rect(rectCorners[0].x, rectCorners[0].y, rectCorners[2].x - rectCorners[0].x, rectCorners[2].y - rectCorners[0].y);
            Rect viewportRect = new Rect(viewportCorners[0].x, viewportCorners[0].y, viewportCorners[2].x - viewportCorners[0].x, viewportCorners[2].y - viewportCorners[0].y);

            return rect.Overlaps(viewportRect);
        }

        /// <summary>
        /// Returns true if a given RectTransform is above the center of a viewport.
        /// (Assumes the RectTransform and viewport are axis aligned.) 
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
        /// (Assumes the RectTransform and viewport are axis aligned.) 
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
