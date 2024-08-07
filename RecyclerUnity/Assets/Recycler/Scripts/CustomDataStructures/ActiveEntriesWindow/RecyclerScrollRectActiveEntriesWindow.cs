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
        public (int Start, int End)? VisibleIndexRange { get; private set; }

        /// <summary>
        /// The range of entry indices contained in the start cache. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? StartCacheIndexRange { get; private set; }

        /// <summary>
        /// The range of entry indices contained in the end cache. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? EndCacheIndexRange { get; private set; }

        /// <summary>
        /// The range of indices of active entries: both visible and cached. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? ActiveEntriesRange => !StartCacheIndexRange.HasValue && !VisibleIndexRange.HasValue && !EndCacheIndexRange.HasValue ?
            null : 
            (StartCacheIndexRange?.Start ?? VisibleIndexRange?.Start ?? 0, 
                EndCacheIndexRange?.End ?? VisibleIndexRange.Value.End);

        /// <summary>
        /// Whether the window of visible entries have changed, also implying the start and end cache have changed.
        /// </summary>
        public bool AreVisibleEntriesDirty { get; private set; }
        
        /// <summary>
        /// The current size of the underlying data that the window moves over.
        /// </summary>
        private int CurrentDataSize
        {
            get => _currentDataSize;
            set
            {
                AreVisibleEntriesDirty = AreVisibleEntriesDirty || value != _currentDataSize;
                _currentDataSize = value;
            }
        }
        
        private readonly int _numCached;
        
        private int _currentDataSize;

        public void SetVisibleRangeUpdateCaches((int Start, int End) newVisibleRange)
        {
            Debug.Log("SETTING " + newVisibleRange);
            AreVisibleEntriesDirty = AreVisibleEntriesDirty || 
                      !VisibleIndexRange.HasValue ||
                      VisibleIndexRange.Value.Start != newVisibleRange.Start || 
                      VisibleIndexRange.Value.End != newVisibleRange.End;

            VisibleIndexRange = newVisibleRange;
            
            // Start cache
            StartCacheIndexRange = newVisibleRange.Start == 0 ?
                null :
                (Mathf.Max(newVisibleRange.Start - _numCached, 0), newVisibleRange.Start - 1);

            // End Cache
            EndCacheIndexRange = newVisibleRange.End == CurrentDataSize - 1 ?
                null :
                (newVisibleRange.End + 1, Mathf.Min( newVisibleRange.End + _numCached, CurrentDataSize - 1));
        }

        public void ClearVisibleRangeUpdateCaches(bool clearStartCache = false, bool clearEndCache = false)
        {
            if (VisibleIndexRange != null)
            {
                VisibleIndexRange = null;
                AreVisibleEntriesDirty = true;
            }

            if (clearStartCache)
            {
                StartCacheIndexRange = null;
            }

            if (clearEndCache)
            {
                EndCacheIndexRange = null;
            }
            
        }

        /// <summary>
        /// Inserts new entries into the window.
        /// By default, the visible state of entries is preserved: what was visible stays visible and vice-versa.
        /// New entries are not considered visible (the range does change) if they fall outside of it.
        /// </summary>
        /// <param name="index"> The index to insert the new entries at. </param>
        /// <param name="num"> The number of entries to insert. </param>
        public void InsertRange(int index, int num)
        {
            // Increase the size of the window
            CurrentDataSize += num;

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

            SetVisibleRangeUpdateCaches(shiftedVisibleIndices);
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

            if (!VisibleIndexRange.HasValue)
            {
                return;
            }

            // If we've deleted everything then nothing is visible
            if (CurrentDataSize == 0)
            {
                ClearVisibleRangeUpdateCaches(true, true);
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

            SetVisibleRangeUpdateCaches(shiftedVisibleIndices);
        }

        /// <summary>
        /// Resets the window to its initial state with no underlying data.
        /// </summary>
        public void Reset()
        {
            ClearVisibleRangeUpdateCaches(true, true);
            CurrentDataSize = 0;
            AreVisibleEntriesDirty = false;
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
        /// Relays that we have checked the changed range of visible entries and are now waiting for the next change.
        /// </summary>
        public void SetVisibleEntriesNonDirty()
        {
            AreVisibleEntriesDirty = false;
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
