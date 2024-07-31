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
        public static bool IsInViewport(RectTransform rectTransform, BoxCollider viewportCollider)
        {
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);
            return worldCorners.Any(viewportCollider.ContainsPoint);
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
