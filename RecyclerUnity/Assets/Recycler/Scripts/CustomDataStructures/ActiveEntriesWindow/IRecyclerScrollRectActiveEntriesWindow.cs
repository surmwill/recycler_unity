using System.Collections.Generic;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Interface for the user to query the various index ranges of active entries in the recycler.
    /// </summary>
    public interface IRecyclerScrollRectActiveEntriesWindow : IEnumerable<int>
    {
        /// <summary>
        /// Returns true if the window exists, that is, we have some underlying recycler data to have a window over in the first place.
        /// </summary>
        bool Exists { get; }

        /// <summary>
        /// The range of entry indices that are visible. Null if the range is empty.
        /// </summary>
        (int Start, int End)? VisibleIndexRange { get; }

        /// <summary>
        /// The range of entry indices contained in the start cache. Null if the range is empty.
        /// </summary>
        (int Start, int End)? StartCacheIndexRange { get; }

        /// <summary>
        /// The range of entry indices contained in the end cache. Null if the range is empty.
        /// </summary>
        (int Start, int End)? EndCacheIndexRange { get; }

        /// <summary>
        /// The range of indices of active entries: both visible and cached. Null if the range is empty.
        /// </summary>
        (int Start, int End)? ActiveEntriesRange { get; }

        /// <summary>
        /// Returns information about the current ranges of entry indices
        /// </summary>
        string PrintRanges();
    }
}
