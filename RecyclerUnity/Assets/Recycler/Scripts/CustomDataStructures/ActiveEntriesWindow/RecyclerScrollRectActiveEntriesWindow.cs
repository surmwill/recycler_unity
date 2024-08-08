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
        public bool Exists => CurrentDataSize > 0;

        /// <summary>
        /// The range of entry indices that are visible. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? VisibleIndexRange
        {
            get => _visibleIndexRange; 
            set
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

        private int CurrentDataSize
        {
            get => _currentDataSize;
            set
            {
                AreActiveEntriesDirty = AreActiveEntriesDirty || _currentDataSize != value;
                _currentDataSize = value;
            }
        }

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
            bool isFirstData = CurrentDataSize == 0;
            
            // Increase the size of the window
            CurrentDataSize += num;

            // The first inserted entries get put into the end cache
            if (isFirstData)
            {
                EndCacheIndexRange = (0, Mathf.Min(_numCached - 1, CurrentDataSize - 1));
                return;
            }

            // If there's visible entries, then update those and then the caches based on the updated range.
            if (VisibleIndexRange.HasValue)
            {
                VisibleIndexRange = InsertIndicesToRange(VisibleIndexRange, index, num);
                UpdateCachesFromVisibleRange();
                return;
            }
            
            // If there's no visible entries, but entries are still in the start cache, then we have a full-screen endcap.
            // Fill up the start cache with the ending entries.
            if (StartCacheIndexRange.HasValue)
            {
                StartCacheIndexRange = TrimRange((0, CurrentDataSize - 1), CurrentDataSize - 1, _numCached, true);
            }
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
            CurrentDataSize--;
            
            // If there's visible entries, then update those and then the caches based on the updated range.
            if (VisibleIndexRange.HasValue)
            {
                VisibleIndexRange = RemoveIndexFromRange(VisibleIndexRange, index);
                UpdateCachesFromVisibleRange();
                return;
            }

            // If there's no visible entries, but entries are still in the start cache, then we have a full-screen endcap.
            // Fill up the start cache with the ending entries.
            if (StartCacheIndexRange.HasValue)
            {
                StartCacheIndexRange = TrimRange((0, CurrentDataSize - 1), CurrentDataSize - 1, _numCached, true);
            }
        }

        /// <summary>
        /// Resets the window to its initial state with no underlying data.
        /// </summary>
        public void Reset()
        {
            CurrentDataSize = 0;
            (StartCacheIndexRange, VisibleIndexRange, EndCacheIndexRange) = (null, null, null);
            AreActiveEntriesDirty = false;
        }

        /// <summary>
        /// Based on the visible indices, determines what should be in the start and end caches.
        /// </summary>
        public void UpdateCachesFromVisibleRange()
        {
            if (!VisibleIndexRange.HasValue)
            {
                StartCacheIndexRange = null;
                EndCacheIndexRange = CurrentDataSize == 0 ? null : (0, Mathf.Min(_numCached - 1, CurrentDataSize - 1));
                return;
            }

            (int Start, int End) visibleIndexRange = VisibleIndexRange.Value;
            
            StartCacheIndexRange = visibleIndexRange.Start == 0 ?
                null :
                (Mathf.Max(visibleIndexRange.Start - _numCached, 0), visibleIndexRange.Start - 1);
            
            EndCacheIndexRange = visibleIndexRange.End == CurrentDataSize - 1 ?
                null :
                (visibleIndexRange.End + 1, Mathf.Min(visibleIndexRange.End + _numCached, CurrentDataSize - 1));
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

        public RecyclerScrollRectActiveEntriesWindow(int numCached)
        {
            _numCached = numCached;
        }

        #region Enumeration

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
        
        #endregion

        #region StaticRangeHelpers
        
        private static (int Start, int End)? TrimRange((int Start, int End)? range, int maxIndex, int maxSize, bool fromStart)
        {
            if (!range.HasValue)
            {
                return null;
            }
            
            (int Start, int End) newRange = (range.Value.Start, Mathf.Min(range.Value.End, maxIndex));
            int diff = maxSize - (newRange.End - newRange.Start + 1);
            
            if (diff >= 0)
            {
                return newRange;
            }

            if (fromStart)
            {
                newRange.Start += Mathf.Abs(diff);
            }
            else
            {
                newRange.End -= Mathf.Abs(diff);
            }

            return newRange;
        }

        private static (int Start, int End)? InsertIndicesToRange((int Start, int End)? range, int index, int num)
        {
            if (!range.HasValue)
            {
                return null;
            }

            (int Start, int End) newRange = range.Value;
            
            // Adjust end
            if (index <= newRange.End)
            {
                newRange.End += num;
            }
            
            // Adjust start
            if (index <= newRange.Start)
            {
                newRange.Start += num;
            }

            return newRange;
        }
        
        private static (int Start, int End)? RemoveIndexFromRange((int Start, int End)? fromRange, int index)
        {
            if (!fromRange.HasValue)
            {
                return null;
            }

            (int Start, int End) newRange = fromRange.Value;
            if (newRange.Start == newRange.End && newRange.End == index)
            {
                return null;
            }

            // Adjust end
            if (index <= newRange.End)
            {
                newRange.End--;
            }
            
            // Adjust start
            if (index < newRange.Start)
            {
                newRange.Start--;
            }
            else if (index == newRange.Start)
            {
                newRange.Start = Mathf.Min(newRange.Start + 1, newRange.End);
            }

            return newRange;
        }
        
        private static bool IsRangeDifferent((int Start, int End)? range1, (int Start, int End)? range2)
        {
            return range1?.Start != range2?.Start || range1?.End != range2?.End;
        }
        
        #endregion
    }
}
