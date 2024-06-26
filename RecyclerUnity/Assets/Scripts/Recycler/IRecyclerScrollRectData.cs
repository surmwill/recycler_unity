using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for data sent to the RecyclerScrollRect. Each piece of data must provide a unique key relative to the full list of data
/// </summary>
public interface IRecyclerScrollRectData<TEntryDataKey>
{
    /// <summary>
    /// The key identifying this piece of data
    /// </summary>
    TEntryDataKey Key { get; }
}
