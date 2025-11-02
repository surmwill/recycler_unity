using System.Collections.Generic;
using System.Linq;
using Swill.Recycler;

public static class IRecyclerScrollRectActiveEntriesWindowExtensions
{
    /// <summary>
    /// Returns the keys of the currently active entries
    /// </summary>
    /// <param name="activeEntriesWindow"> The index window </param>
    /// <returns> The keys of the currently active entries </returns>
    public static IEnumerable<TKeyEntryData> GetActiveKeys<TKeyEntryData, TEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> activeEntriesWindow) 
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        return GetKeyRange(activeEntriesWindow, activeEntriesWindow.ActiveEntriesRange);
    }
    
    /// <summary>
    /// Returns the keys of the currently visible entries
    /// </summary>
    /// <param name="activeEntriesWindow"> The index window </param>
    /// <returns> The keys of the currently visible entries </returns>
    public static IEnumerable<TKeyEntryData> GetVisibleKeys<TKeyEntryData, TEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> activeEntriesWindow) 
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        return GetKeyRange(activeEntriesWindow, activeEntriesWindow.VisibleIndexRange);
    }
    
    /// <summary>
    /// Returns the keys of the entries currently in the start cache
    /// </summary>
    /// <param name="activeEntriesWindow"> The index window </param>
    /// <returns> The keys of the entries currently in the start cache </returns>
    public static IEnumerable<TKeyEntryData> GetStartCacheKeys<TKeyEntryData, TEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> activeEntriesWindow) 
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        return GetKeyRange(activeEntriesWindow, activeEntriesWindow.StartCacheIndexRange);
    }
    
    /// <summary>
    /// Returns the keys of the entries currently in the end cache
    /// </summary>
    /// <param name="activeEntriesWindow"> The index window </param>
    /// <returns> The keys of the entries currently in the end cache </returns>
    public static IEnumerable<TKeyEntryData> GetEndCacheKeys<TKeyEntryData, TEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> activeEntriesWindow) 
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        return GetKeyRange(activeEntriesWindow, activeEntriesWindow.EndCacheIndexRange);
    }
    
    /// <summary>
    /// Returns true if the entry with the given key is visible.
    /// </summary>
    /// <param name="activeEntriesWindow"> The index window </param>
    /// <param name="key"> The key of the entry </param>
    /// <returns> True if the entry with the given is visible. </returns>
    public static bool IsKeyVisible<TKeyEntryData, TEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> activeEntriesWindow, TKeyEntryData key)
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        return activeEntriesWindow.Recycler.TryGetActiveEntryWithKey(key, out RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry) && 
               activeEntriesWindow.IsVisible(entry.Index);
    }
    
    /// <summary>
    /// Returns true if the entry with the given key is in the start cache.
    /// </summary>
    /// <param name="activeEntriesWindow"> The index window </param>
    /// <param name="key"> The key of the entry </param>
    /// <returns> True if the entry with the given key is in the start cache. </returns>
    public static bool IsKeyInStartCache<TKeyEntryData, TEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> activeEntriesWindow, TKeyEntryData key)
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        return activeEntriesWindow.Recycler.TryGetActiveEntryWithKey(key, out RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry) && 
               activeEntriesWindow.IsInStartCache(entry.Index);
    }
    
    /// <summary>
    /// Returns true if the entry with the given key is in the end cache.
    /// </summary>
    /// <param name="activeEntriesWindow"> The index window </param>
    /// <param name="key"> The key of the entry </param>
    /// <returns> True if the entry with the given key is in the end cache. </returns>
    public static bool IsKeyInEndCache<TKeyEntryData, TEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> activeEntriesWindow, TKeyEntryData key)
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        return activeEntriesWindow.Recycler.TryGetActiveEntryWithKey(key, out RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry) && 
               activeEntriesWindow.IsInEndCache(entry.Index);
    }
    
    /// <summary>
    /// Returns true if the entry with the given key is active (visible, in the start cache, or in the end cache)
    /// </summary>
    /// <param name="activeEntriesWindow"> The index window </param>
    /// <param name="key"> The key of the entry </param>
    /// <returns> True if the entry with the given key is active. </returns>
    public static bool ContainsKey<TKeyEntryData, TEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> activeEntriesWindow, TKeyEntryData key)
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        return IsKeyVisible(activeEntriesWindow, key) || IsKeyInStartCache(activeEntriesWindow, key) || IsKeyInEndCache(activeEntriesWindow, key);
    }
    
    /// <summary>
    /// Returns information about the current ranges of entry keys
    /// </summary>
    /// <returns> A string detailing the current ranges of entry keys </returns>
    public static string PrintKeyRanges<TKeyEntryData, TEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> activeEntriesWindow)
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        IEnumerable<TKeyEntryData> startCacheKeys = GetStartCacheKeys(activeEntriesWindow);
        IEnumerable<TKeyEntryData> visibleKeys = GetVisibleKeys(activeEntriesWindow);
        IEnumerable<TKeyEntryData> endCacheKeys = GetEndCacheKeys(activeEntriesWindow);
        
        return
            $"Start Cache Range: {(!startCacheKeys.Any() ? "[]" : $"[{startCacheKeys.First()},{startCacheKeys.Last()}]")}\n" +
            $"Visible Index Range: {(!visibleKeys.Any() ? "[]" : $"[{visibleKeys.First()},{visibleKeys.Last()}]")}\n" +
            $"End Cache Range: {(!endCacheKeys.Any() ? "[]" : $"[{endCacheKeys.First()},{endCacheKeys.Last()}]")}";
    }

    private static IEnumerable<TKeyEntryData> GetKeyRange<TKeyEntryData, TEntryData>(
        IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> activeEntriesWindow, (int Start, int End)? range)
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        if (!range.HasValue)
        {
            return Enumerable.Empty<TKeyEntryData>();
        }
        
        (int Start, int End) = range.Value;
        return activeEntriesWindow.Recycler.DataForEntries.Skip(Start).Take(End - Start + 1).Select(entryData => entryData.Key);
    }
}
