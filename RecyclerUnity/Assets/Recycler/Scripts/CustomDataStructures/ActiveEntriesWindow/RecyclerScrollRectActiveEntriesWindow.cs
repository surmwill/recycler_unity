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
        public bool Exists => _virContainer.CurrentDataSize > 0;

        /// <summary>
        /// The range of entry indices that are visible. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? VisibleIndexRange
        {
            get => _virContainer.VisibleIndexRange;
            set => _virContainer.VisibleIndexRange = value;
        }

        /// <summary>
        /// The range of entry indices contained in the start cache. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? StartCacheIndexRange => !Exists || !VisibleIndexRange.HasValue || VisibleIndexRange.Value.Start == 0 ?
                null :
                (Mathf.Max(VisibleIndexRange.Value.Start - _numCached, 0), VisibleIndexRange.Value.Start - 1);

        /// <summary>
        /// The range of entry indices contained in the end cache. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? EndCacheIndexRange => !Exists || (VisibleIndexRange.HasValue && VisibleIndexRange.Value.End == _virContainer.CurrentDataSize - 1) ?
            null :
            (VisibleIndexRange?.End + 1 ?? 0, Mathf.Min(VisibleIndexRange.HasValue ? VisibleIndexRange.Value.End + _numCached : _numCached - 1, _virContainer.CurrentDataSize - 1));

        /// <summary>
        /// The range of indices of active entries: both visible and cached. Null if the range is empty.
        /// </summary>
        public (int Start, int End)? ActiveEntriesRange => !Exists ?
            null :
            (StartCacheIndexRange?.Start ?? VisibleIndexRange?.Start ?? 0, EndCacheIndexRange?.End ?? VisibleIndexRange.Value.End);

        /// <summary>
        /// Whether the window of active entries has changed.
        /// </summary>
        public bool IsDirty { get; private set; }

        private readonly int _numCached;
        private readonly VisibleIndexRangeContainer _virContainer;
        
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
            _virContainer.CurrentDataSize += num;

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

            VisibleIndexRange = shiftedVisibleIndices;
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
            _virContainer.CurrentDataSize--;

            if (!VisibleIndexRange.HasValue)
            {
                return;
            }

            // If we've deleted everything then nothing is visible
            if (_virContainer.CurrentDataSize == 0)
            {
                VisibleIndexRange = null;
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

            VisibleIndexRange = shiftedVisibleIndices;
        }

        /// <summary>
        /// Resets the window to its initial state with no underlying data.
        /// </summary>
        public void Reset()
        {
            VisibleIndexRange = null;
            _virContainer.CurrentDataSize = 0;
            IsDirty = false;
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
        /// Relays that we have checked the current range of active entries, done any work we need to,
        /// and are now waiting on the window to become dirty again (the range of active entries changes).
        /// </summary>
        public void SetNonDirty()
        {
            IsDirty = false;
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
            _virContainer = new VisibleIndexRangeContainer(this);
        }

        /// <summary>
        /// This class encapsulates a set of properties that could technically live in the outer class just fine.
        /// Its purpose is to ensure the backing fields are never modified directly, but only through the property setters.
        /// </summary>
        private class VisibleIndexRangeContainer
        {
            private readonly RecyclerScrollRectActiveEntriesWindow _entriesWindow;

            private (int Start, int End)? _visibleIndexRange;
            private int _currentDataSize;

            /// <summary>
            /// The range of entry indices that are visible. Null if the range is empty.
            /// </summary>
            public (int Start, int End)? VisibleIndexRange
            {
                get => _visibleIndexRange;
                set
                {
                    (int Start, int End)? newVisibleIndices = value;
                    _entriesWindow.IsDirty = _entriesWindow.IsDirty ||
                                             _visibleIndexRange.HasValue != newVisibleIndices.HasValue ||
                                             _visibleIndexRange?.Start != newVisibleIndices?.Start ||
                                             _visibleIndexRange?.End != newVisibleIndices?.End;

                    #if UNITY_EDITOR
                    if (newVisibleIndices.HasValue && newVisibleIndices.Value.Start > newVisibleIndices.Value.End)
                    {
                        Debug.LogError(
                            $"The visible start index \"{newVisibleIndices.Value.Start}\" should not be greater than the end index \"{newVisibleIndices.Value.End}\"");
                    }
                    #endif

                    _visibleIndexRange = newVisibleIndices;
                }
            }

            /// <summary>
            /// The current size of the underlying data that the window moves over.
            /// </summary>
            public int CurrentDataSize
            {
                get => _currentDataSize;
                set
                {
                    _entriesWindow.IsDirty = _entriesWindow.IsDirty || value != _currentDataSize;
                    _currentDataSize = value;
                }
            }

            public VisibleIndexRangeContainer(RecyclerScrollRectActiveEntriesWindow entriesWindow)
            {
                _entriesWindow = entriesWindow;
            }
        }
    }
}
