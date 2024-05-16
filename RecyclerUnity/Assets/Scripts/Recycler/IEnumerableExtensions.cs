using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Extensions for IEnumerable
/// </summary>
public static class IEnumerableExtensions
{
    /// <summary>
    /// Given an IEnumerable returns that same IEnumerable except with each piece of data paired with its index
    /// </summary>
    public static IEnumerable<(T, int)> ZipWithIndex<T>(this IEnumerable<T> enumerable)
    {
        // Note that Zip will stop at the end of the shortest IEnumerable (very likely not int.MaxValue)
        return enumerable.Zip(Enumerable.Range(0, int.MaxValue), (data, index) => (data, index));
    }
}
