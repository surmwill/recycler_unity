using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for the user to query the indices of active entries in the Recycler
/// </summary>
public interface IRecyclerScrollRectActiveEntriesWindow
{
    /// <summary>
    /// Returns true if the window exists, that is, we've added some underlying data to have a window over in the first place
    /// </summary>
    public bool Exists { get; }

    /// <summary>
    /// The starting index of the range of visible entries. Returns -1 if no entries are visible.
    /// </summary>
    public int VisibleStartIndex { get; }

    /// <summary>
    /// The ending index of the range of visible entries. Returns -1 if no entries are visible
    /// </summary>
    public int VisibleEndIndex { get; }

    /// <summary>
    /// The starting index of the range of entries contained in the start cache. The entry before the first visible entry is when the range stops.
    /// If the start cache is empty then -1 is returned.
    /// </summary>
    public int StartCacheStartIndex { get; }

    /// <summary>
    /// The ending index of the range of entries contained in the end cache. The entry after the last visible entry is when the range stops.
    /// If the end cache is empty then -1 is returned.
    /// </summary>
    public int EndCacheEndIndex { get; }

    /// <summary>
    /// The range of entry indices that are visible
    /// </summary>
    public (int Start, int End) VisibleIndexRange { get; }

    /// <summary>
    /// The range of entry indices contained in the start cache
    /// </summary>
    public (int Start, int End) StartCacheIndexRange { get; }

    /// <summary>
    /// The range of entry indices contained in the end cache 
    /// </summary>
    public (int Start, int End) EndCacheIndexRange { get; }

    /// <summary>
    /// The range of active entries: both visible and cached
    /// </summary>
    public (int Start, int End) ActiveEntriesRange { get; }
}
