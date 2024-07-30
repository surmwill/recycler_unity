using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Extensions for BoxColliders.
    /// </summary>
    public static class BoxColliderExtensions
    {
        /// <summary>
        /// Returns true if a BoxCollider contains a point.
        /// </summary>
        /// <param name="boxCollider"> The BoxCollider. </param>
        /// <param name="point"> The point. </param>
        /// <returns> True if the BoxCollider contains the point </returns>
        public static bool ContainsPoint(this BoxCollider boxCollider, Vector3 point)
        {
            return boxCollider.ClosestPoint(point) == point;
        }
    }
}
