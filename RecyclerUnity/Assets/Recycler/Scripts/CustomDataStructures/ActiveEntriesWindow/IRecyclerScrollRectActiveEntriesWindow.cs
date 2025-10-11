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
        /// Returns true if the given index is visible.
        /// </summary>
        /// <param name="index"> The index to test if it is visible. </param>
        /// <returns> True if the index is visible. </returns>
        public bool IsVisible(int index)
        {
            return VisibleIndexRange.HasValue && index >= VisibleIndexRange.Value.Start && index <= VisibleIndexRange.Value.End;
        }

        /// <summary>
        /// Returns true if the given index is in the start cache.
        /// </summary>
        /// <param name="index"> The index to test if it is in the start cache. </param>
        /// <returns> True if the index is in the start cache. </returns>
        public bool IsInStartCache(int index);

        /// <summary>
        /// Returns true if the given index is in the end cache.
        /// </summary>
        /// <param name="index"> The index to test if it is in the end cache </param>
        /// <returns> True if the index is in the end cache. </returns>
        public bool IsInEndCache(int index);

        /// <summary>
        /// Returns true if the given index is an active entry, either visible or cached. 
        /// </summary>
        /// <param name="index"> The index to test if it is an active entry. </param>
        /// <returns> True if the index is of an active entry. </returns>
        public bool Contains(int index);

        /// <summary>
        /// Returns information about the current ranges of entry indices
        /// </summary>
        string PrintRanges();
    }
}
