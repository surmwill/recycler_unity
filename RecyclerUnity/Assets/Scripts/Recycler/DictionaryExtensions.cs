using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Extensions for dictionaries
/// </summary>
public static class DictionaryExtensions 
{
    /// <summary>
    /// Removes all entries that satisfy a predicate
    /// </summary>
    public static IEnumerable<KeyValuePair<TKey, TValue>> RemoveWhere<TKey, TValue>(this Dictionary<TKey, TValue> d, Func<KeyValuePair<TKey, TValue>, bool> pred)
    {
        List<KeyValuePair<TKey, TValue>> removeEntries = null;
        foreach (KeyValuePair<TKey,TValue> entry in d.Where(pred))
        {
            (removeEntries ??= new List<KeyValuePair<TKey, TValue>>()).Add(entry);
        }

        if (removeEntries == null)
        {
            return Enumerable.Empty<KeyValuePair<TKey, TValue>>();
        }
        
        foreach (TKey key in removeEntries.Select(r => r.Key))
        {
            d.Remove(key);
        }

        return removeEntries;
    }
}
