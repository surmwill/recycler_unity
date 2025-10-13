using System.Collections.Generic;
using System.Linq;
using RecyclerScrollRect;

public static class IRecyclerScrollRectActiveEntriesWindowExtensions
{
    public static IEnumerable<TKeyEntryData> GetActiveKeys<TEntryData, TKeyEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TEntryData, TKeyEntryData> activeEntriesWindow) 
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        return GetKeyRange(activeEntriesWindow, activeEntriesWindow.ActiveEntriesRange);
    }
    
    public static IEnumerable<TKeyEntryData> GetVisibleKeys<TEntryData, TKeyEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TEntryData, TKeyEntryData> activeEntriesWindow) 
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        return GetKeyRange(activeEntriesWindow, activeEntriesWindow.VisibleIndexRange);
    }
    
    public static IEnumerable<TKeyEntryData> GetStartCacheKeys<TEntryData, TKeyEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TEntryData, TKeyEntryData> activeEntriesWindow) 
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        return GetKeyRange(activeEntriesWindow, activeEntriesWindow.StartCacheIndexRange);
    }
    
    public static IEnumerable<TKeyEntryData> GetEndCacheKeys<TEntryData, TKeyEntryData>(this IRecyclerScrollRectActiveEntriesWindow<TEntryData, TKeyEntryData> activeEntriesWindow) 
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        return GetKeyRange(activeEntriesWindow, activeEntriesWindow.EndCacheIndexRange);
    }

    private static IEnumerable<TKeyEntryData> GetKeyRange<TEntryData, TKeyEntryData>(
        IRecyclerScrollRectActiveEntriesWindow<TEntryData, TKeyEntryData> activeEntriesWindow, (int Start, int End)? range)
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
