using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Interface for data sent to the RecyclerScrollRect. Each piece of data must provide a unique key relative to the full list of data
    /// </summary>
    public interface IRecyclerScrollRectData<TEntryDataKey>
    {
        /// <summary>
        /// A unique key identifying this piece of data
        /// </summary>
        TEntryDataKey Key { get; }
    }
}
