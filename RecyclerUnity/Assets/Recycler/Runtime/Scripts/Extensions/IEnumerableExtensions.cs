using System.Collections.Generic;
using System.Linq;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Extensions for IEnumerable.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Given an IEnumerable, returns that same IEnumerable except with each piece of data paired with its corresponding index.
        /// </summary>
        /// <param name="enumerable"> The IEnumerable. </param>
        /// <typeparam name="T"> The type of the IEnumerable. </typeparam>
        /// <returns> The IEnumerable with each element paired with its corresponding index. </returns>
        public static IEnumerable<(T, int)> ZipWithIndex<T>(this IEnumerable<T> enumerable)
        {
            // Note that Zip will stop at the end of the shortest IEnumerable (likely not int.MaxValue).
            return enumerable.Zip(Enumerable.Range(0, int.MaxValue), (data, index) => (data, index));
        }
    }
}
