using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Extensions for working with Vector2s
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// Returns the vector with a new x value
        /// </summary>
        public static Vector2 WithX(this Vector2 vec, float x)
        {
            return new Vector2(x, vec.y);
        }

        /// <summary>
        /// Returns the vector with a new y value
        /// </summary>
        public static Vector2 WithY(this Vector2 vec, float y)
        {
            return new Vector2(vec.x, y);
        }

        /// <summary>
        /// Returns a Vector3 with the Vector2 values and an additional specified z
        /// </summary>
        public static Vector3 WithZ(this Vector2 vec, float z)
        {
            return new Vector3(vec.x, vec.y, z);
        }
    }
}
