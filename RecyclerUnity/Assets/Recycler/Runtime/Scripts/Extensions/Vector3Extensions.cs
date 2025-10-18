using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Extensions for Vector3s.
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Returns a Vector3 with a new x value.
        /// </summary>
        /// <param name="vec"> The Vector3 that needs a new x value. </param>
        /// <param name="x"> The new x value of the Vector3. </param>
        /// <returns> The Vector3 but with a new x value. </returns>
        public static Vector3 WithX(this Vector3 vec, float x)
        {
            return new Vector3(x, vec.y, vec.z);
        }

        /// <summary>
        /// Returns a Vector3 with a new y value.
        /// </summary>
        /// <param name="vec"> The Vector3 that needs a new y value. </param>
        /// <param name="y"> The new y value of the Vector3. </param>
        /// <returns> The Vector3 but with a new y value. </returns>
        public static Vector3 WithY(this Vector3 vec, float y)
        {
            return new Vector3(vec.x, y, vec.z);
        }

        /// <summary>
        /// Returns a Vector3 with a new z value.
        /// </summary>
        /// <param name="vec"> The Vector3 that needs a new z value. </param>
        /// <param name="z"> The new z value of the Vector3. </param>
        /// <returns> The Vector3 but with a new z value. </returns>
        public static Vector3 WithZ(this Vector3 vec, float z)
        {
            return new Vector3(vec.x, vec.y, z);
        }
        
        /// <summary>
        /// Normal printing of Vector3s rounds the values, but this doesn't.
        /// </summary>
        /// <param name="vec"> The Vector3 to print. </param>
        /// <returns> A string representation of the Vector3. </returns>
        public static string PrecisePrint(this Vector3 vec)
        {
            return $"({vec.x},{vec.y},{vec.z})";
        }
    }
}
