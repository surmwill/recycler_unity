using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Represents a sliding range of entry indices currently active in the recycler: visible or cached. 
    /// </summary>
    public class RecyclerScrollRectActiveEntriesWindow : IRecyclerScrollRectActiveEntriesWindow
    {
        /// <summary>
        /// Returns true if the window exists, that is, we have some underlying recycler data to have a window over in the first place.
        /// </summary>
        public bool Exists => _currentDataSize > 0;

        /// <summary>
        /// The range of entry indices that are visible. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? VisibleIndexRange
        {
            get => _visibleIndexRange;
            private set
            {
                AreActiveEntriesDirty = AreActiveEntriesDirty || IsRangeDifferent(value, _visibleIndexRange);
                _visibleIndexRange = value;
            }
        }

        /// <summary>
        /// The range of entry indices contained in the start cache. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? StartCacheIndexRange
        {
            get => _startCacheIndexRange;
            private set
            {
                AreActiveEntriesDirty = AreActiveEntriesDirty || IsRangeDifferent(value, _startCacheIndexRange);
                _startCacheIndexRange = value;
            }
        }

        /// <summary>
        /// The range of entry indices contained in the end cache. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? EndCacheIndexRange
        {
            get => _endCacheIndexRange;
            private set
            {
                AreActiveEntriesDirty = AreActiveEntriesDirty || IsRangeDifferent(value, _endCacheIndexRange);
                _endCacheIndexRange = value;
            }
        }

        /// <summary>
        /// The range of indices of active entries: both visible and cached. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? ActiveEntriesRange => !StartCacheIndexRange.HasValue && !VisibleIndexRange.HasValue && !EndCacheIndexRange.HasValue ?
            null : 
            (StartCacheIndexRange?.Start ?? VisibleIndexRange?.Start ?? 0, 
                EndCacheIndexRange?.End ?? VisibleIndexRange?.End ?? StartCacheIndexRange.Value.End);

        /// <summary>
        /// Whether the range of active entries has changed.
        /// </summary>
        public bool AreActiveEntriesDirty { get; private set; }

        private (int Start, int End)? _visibleIndexRange;
        private (int Start, int End)? _startCacheIndexRange;
        private (int Start, int End)? _endCacheIndexRange;
        
        private int _currentDataSize;
        private readonly int _numCached;

        /// <summary>
        /// Inserts new entries into the window.
        /// By default, the visible state of entries is preserved: what was visible stays visible and vice-versa.
        /// New entries are not considered visible (the range does change) if they fall outside of it.
        /// </summary>
        /// <param name="index"> The index to insert the new entries at. </param>
        /// <param name="num"> The number of entries to insert. </param>
        public void InsertRange(int index, int num)
        {
            bool isFirstInsert = _currentDataSize == 0;
            
            // Increase the size of the window
            _currentDataSize += num;

            // The first inserted entries get put into the end cache
            if (isFirstInsert)
            {
                EndCacheIndexRange = (0, Mathf.Min(_numCached - 1, _currentDataSize - 1));
            }

            if (!VisibleIndexRange.HasValue)
            {
                return;
            }

            // Shift the currently visible window to accomodate the added entries
            (int Start, int End) shiftedVisibleIndices = VisibleIndexRange.Value;
            
            if (index <= VisibleIndexRange.Value.End)
            {
                shiftedVisibleIndices.End += num;
            }
            
            if (index <= VisibleIndexRange.Value.Start)
            {
                shiftedVisibleIndices.Start += num;
            }

            SetVisibleRangeAndUpdateCaches(shiftedVisibleIndices);
        }
        
        /// <summary>
        /// Removes an entry from the window.
        /// By default, the visible state of entries is preserved: what was visible stays visible and vice-versa.
        /// No new entries become visible (the range does not change) from removal.
        /// </summary>
        /// <param name="index"> The index of the entry to remove. </param>
        public void Remove(int index)
        {
            // Decrease the size of the window
            _currentDataSize--;

            // If we've deleted everything then nothing is visible
            if (_currentDataSize == 0)
            {
                ClearVisibleRangeAndCaches();
                return;
            }
            
            if (!VisibleIndexRange.HasValue)
            {
                return;
            }

            // Shift the current window to accomodate the removed entry
            (int Start, int End) shiftedVisibleIndices = VisibleIndexRange.Value;

            if (index <= VisibleIndexRange.Value.End)
            {
                shiftedVisibleIndices.End--;
            }

            if (index < VisibleIndexRange.Value.Start)
            {
                shiftedVisibleIndices.Start--;
            }
            else if (index == VisibleIndexRange.Value.Start)
            {
                shiftedVisibleIndices.Start = Mathf.Min(shiftedVisibleIndices.End, shiftedVisibleIndices.Start + 1);
            }

            SetVisibleRangeAndUpdateCaches(shiftedVisibleIndices);
        }

        /// <summary>
        /// Resets the window to its initial state with no underlying data.
        /// </summary>
        public void Reset()
        {
            _currentDataSize = 0;
            ClearVisibleRangeAndCaches();
            AreActiveEntriesDirty = false;
        }
        
        /// <summary>
        /// Updates the visible range of indices, which, in turn, updates what's in the start and end cache.
        /// </summary>
        /// <param name="newVisibleRange"> The new visible range of indices. </param>
        public void SetVisibleRangeAndUpdateCaches((int Start, int End) newVisibleRange)
        {
            VisibleIndexRange = newVisibleRange;
            
            StartCacheIndexRange = newVisibleRange.Start == 0 ?
                null :
                (Mathf.Max(newVisibleRange.Start - _numCached, 0), newVisibleRange.Start - 1);
            
            EndCacheIndexRange = newVisibleRange.End == _currentDataSize - 1 ?
                null :
                (newVisibleRange.End + 1, Mathf.Min( newVisibleRange.End + _numCached, _currentDataSize - 1));
        }

        /// <summary>
        /// Clears only the visible range of indices.
        /// </summary>
        public void ClearVisibleRange()
        {
            VisibleIndexRange = null;
        }

        /// <summary>
        /// Clears the visible range of indices, as well as those in the start and end cache.
        /// </summary>
        public void ClearVisibleRangeAndCaches()
        {
            ClearVisibleRange();
            StartCacheIndexRange = null;
            EndCacheIndexRange = _currentDataSize == 0 ? null : (0, Mathf.Min(_numCached - 1, _currentDataSize - 1));
        }

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
        public bool IsInStartCache(int index)
        {
            return StartCacheIndexRange.HasValue && index >= StartCacheIndexRange.Value.Start && index <= StartCacheIndexRange.Value.End;
        }

        /// <summary>
        /// Returns true if the given index is in the end cache.
        /// </summary>
        /// <param name="index"> The index to test if it is in the end cache </param>
        /// <returns> True if the index is in the end cache. </returns>
        public bool IsInEndCache(int index)
        {
            return EndCacheIndexRange.HasValue && index >= EndCacheIndexRange.Value.Start && index <= EndCacheIndexRange.Value.End;
        }

        /// <summary>
        /// Returns true if the given index is an active entry, either visible or cached. 
        /// </summary>
        /// <param name="index"> The index to test if it is an active entry. </param>
        /// <returns> True if the index is of an active entry. </returns>
        public bool Contains(int index)
        {
            return IsVisible(index) || IsInStartCache(index) || IsInEndCache(index);
        }

        /// <summary>
        /// Relays that we have checked the changed range of active entries and are now waiting for the next change.
        /// </summary>
        public void SetActiveEntriesNonDirty()
        {
            AreActiveEntriesDirty = false;
        }

        /// <summary>
        /// Returns information about the current ranges of entry indices.
        /// </summary>
        public string PrintRanges()
        {
            return
                $"Visible Index Range: {(!VisibleIndexRange.HasValue ? "[]" : $"[{VisibleIndexRange.Value.Start},{VisibleIndexRange.Value.End}]")}\n" +
                $"Start Cache Range: {(!StartCacheIndexRange.HasValue ? "[]" : $"[{StartCacheIndexRange.Value.Start},{StartCacheIndexRange.Value.End}]")}\n" +
                $"End Cache Range: {(!EndCacheIndexRange.HasValue ? "[]" : $"[{EndCacheIndexRange.Value.Start},{EndCacheIndexRange.Value.End}]")}";
        }
        
        private static bool IsRangeDifferent((int Start, int End)? range1, (int Start, int End)? range2)
        {
            return range1?.Start != range2?.Start || range1?.End != range2?.End;
        }
        
        /// <summary>
        /// Returns the indices of all the active entries in increasing order.
        /// </summary>
        public IEnumerator<int> GetEnumerator()
        {
            if (!ActiveEntriesRange.HasValue)
            {
                return Enumerable.Empty<int>().GetEnumerator();
            }

            (int Start, int End) = ActiveEntriesRange.Value;
            return Enumerable.Range(Start, End - Start + 1).GetEnumerator();
        }

        /// <summary>
        /// Returns the indices of all the active entries in an increasing order.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public RecyclerScrollRectActiveEntriesWindow(int numCached)
        {
            _numCached = numCached;
        }
    }
}
