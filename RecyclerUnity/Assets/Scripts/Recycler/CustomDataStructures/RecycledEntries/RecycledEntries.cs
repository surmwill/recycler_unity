using System.Collections.Generic;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Maintains a dictionary of recycled entries as well as as a queue (a LinkedList) to track which entries have sat in recycling the longest
    /// </summary>
    public class RecycledEntries<TEntryData, TKeyEntryData> where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        // The entries in the recycling pool, placed in a dictionary for quick lookup 
        private Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> _entries = new();

        // The entries (their indices) in the recycling pool, but sorted front to back by whatever entry has been in the pool the longest
        private readonly LinkedList<int> _queueEntries = new();

        // Maps an entries index to its position in the recycling queue
        private Dictionary<int, LinkedListNode<int>> _entriesQueuePosition = new();

        /// <summary>
        /// The recycled entries which can be looked up by their index
        /// </summary>
        public IReadOnlyDictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> Entries => _entries;

        /// <summary>
        /// Adds an entry to the recycling pool
        /// </summary>
        public void Add(int index, RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry)
        {
            _entries.Add(index, entry);

            LinkedListNode<int> insertionQueuePosition = _queueEntries.AddLast(index);
            _entriesQueuePosition.Add(index, insertionQueuePosition);
        }

        /// <summary>
        /// Removes an entry from the recycling pool
        /// </summary>
        public void Remove(int index)
        {
            _entries.Remove(index);

            LinkedListNode<int> queuePosition = _entriesQueuePosition[index];
            _queueEntries.Remove(queuePosition);
            _entriesQueuePosition.Remove(index);
        }

        /// <summary>
        /// Shifts the indices of entries we are bookkeeping
        /// </summary>
        public void ShiftIndices(int startIndex, int shiftAmount)
        {
            Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> shiftedEntries =
                new Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();
            Dictionary<int, LinkedListNode<int>> shiftedQueuePositions = new Dictionary<int, LinkedListNode<int>>();

            foreach ((int index, RecyclerScrollRectEntry<TEntryData, TKeyEntryData> recycledEntry) in _entries)
            {
                int shiftedIndex = index + (index >= startIndex ? shiftAmount : 0);
                LinkedListNode<int> shiftedQueuePosition = _entriesQueuePosition[index];

                if (shiftedIndex != index)
                {
                    recycledEntry.SetIndex(shiftedIndex);
                    shiftedQueuePosition.Value = shiftedIndex;
                }

                shiftedEntries[shiftedIndex] = recycledEntry;
                shiftedQueuePositions[shiftedIndex] = shiftedQueuePosition;
            }

            _entries = shiftedEntries;
            _entriesQueuePosition = shiftedQueuePositions;
        }

        /// <summary>
        /// Returns the entry that has set in the recycling pool the longest
        /// </summary>
        public KeyValuePair<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> GetOldestEntry()
        {
            int oldestIndex = _queueEntries.First.Value;
            return new KeyValuePair<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>(oldestIndex,
                _entries[oldestIndex]);
        }
    }
}