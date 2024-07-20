using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Extension methods for Transforms
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// Returns the children of a transform as an IEnumerable
        /// </summary>
        public static IEnumerable<Transform> Children(this Transform t)
        {
            return t.Cast<Transform>();
        }
    }
}
