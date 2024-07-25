using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Interface for the user to query the indices of active entries in the Recycler
    /// </summary>
    public interface IRecyclerScrollRectActiveEntriesWindow : IEnumerable<int>
    {
        /// <summary>
        /// Returns true if the window exists, that is, we've added some underlying data to have a window over in the first place
        /// </summary>
        public bool Exists { get; }

        /// <summary>
        /// The range of entry indices that are visible. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? VisibleIndexRange { get; }

        /// <summary>
        /// The range of entry indices contained in the start cache. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? StartCacheIndexRange { get; }

        /// <summary>
        /// The range of entry indices contained in the end cache. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? EndCacheIndexRange { get; }

        /// <summary>
        /// The range of active entries: both visible and cached. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? ActiveEntriesRange { get; }
    }
}
