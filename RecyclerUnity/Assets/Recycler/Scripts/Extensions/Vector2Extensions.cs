using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Extensions for working with Vector2s.
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// Returns a Vector2 with a new x value.
        /// </summary>
        /// <param name="vec"> The Vector2 that needs a new x value. </param>
        /// <param name="x"> The new x value of the Vector2. </param>
        /// <returns> The Vector2 but with a new x value. </returns>
        public static Vector2 WithX(this Vector2 vec, float x)
        {
            return new Vector2(x, vec.y);
        }

        /// <summary>
        /// Returns a Vector2 with a new y value.
        /// </summary>
        /// <param name="vec"> The Vector2 that needs a new y value. </param>
        /// <param name="y"> The new y value of the Vector2. </param>
        /// <returns> The Vector2 but with a new y value. </returns>
        public static Vector2 WithY(this Vector2 vec, float y)
        {
            return new Vector2(vec.x, y);
        }

        /// <summary>
        /// Returns a Vector2 with a new z value.
        /// </summary>
        /// <param name="vec"> The Vector2 that needs a new z value. </param>
        /// <param name="z"> The new z value of the Vector2. </param>
        /// <returns> The Vector2 but with a new z value. </returns>
        public static Vector3 WithZ(this Vector2 vec, float z)
        {
            return new Vector3(vec.x, vec.y, z);
        }
    }
}
