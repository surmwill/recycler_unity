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
    /// The range of entry indices that are visible
    /// </summary>
    public (int Start, int End)? VisibleIndexRange { get; }

    /// <summary>
    /// The range of entry indices contained in the start cache
    /// </summary>
    public (int Start, int End)? StartCacheIndexRange { get; }

    /// <summary>
    /// The range of entry indices contained in the end cache 
    /// </summary>
    public (int Start, int End)? EndCacheIndexRange { get; }

    /// <summary>
    /// The range of active entries: both visible and cached
    /// </summary>
    public (int Start, int End)? ActiveEntriesRange { get; }
}
