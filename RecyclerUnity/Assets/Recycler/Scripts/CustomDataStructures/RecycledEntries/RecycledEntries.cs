using System.Collections.Generic;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Maintains a dictionary of recycled entries as well as as a queue (a LinkedList) to track which entries have sat in recycling the longest
    /// </summary>
    public class RecycledEntries<TEntryData, TKeyEntryData> where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        // The entries in the recycling pool, placed in a dictionary for quick lookup.
        private Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> _entries = new();

        // The entries (their indices) in the recycling pool, sorted front-to-back by whatever entry has been in the pool the longest.
        private readonly LinkedList<int> _entryQueue = new();

        // Maps an entry's index to its position in the recycling queue.
        private Dictionary<int, LinkedListNode<int>> _entryIndexToQueuePosition = new();

        /// <summary>
        /// The recycled entries; the key is their index.
        /// </summary>
        public IReadOnlyDictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> Entries => _entries;

        /// <summary>
        /// Adds an entry to the recycling pool.
        /// </summary>
        /// <param name="entry"> The entry to add to the recycling pool. </param>
        public void Add(RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry)
        {
            int index = entry.Index;
            
            // Add the entry to the lookup
            _entries.Add(index, entry);
            
            // Add the entry to the back of the queue
            LinkedListNode<int> insertionQueuePosition = _entryQueue.AddLast(index);
            
            // Map the entry to its position in the queue
            _entryIndexToQueuePosition.Add(index, insertionQueuePosition);
        }

        /// <summary>
        /// Removes an entry from the recycling pool.
        /// </summary>
        /// <param name="index"> The index of the entry to remove. </param>
        public void Remove(int index)
        {
            // Remove the entry from the lookup
            _entries.Remove(index);

            // Find the entries position in the queue
            LinkedListNode<int> queuePosition = _entryIndexToQueuePosition[index];
            
            // Remove the entry from the queue
            _entryQueue.Remove(queuePosition);
            
            // Remove the index -> queue position mapping
            _entryIndexToQueuePosition.Remove(index);
        }

        /// <summary>
        /// Shifts the indices of entries we are bookkeeping like a list.
        /// </summary>
        /// <param name="startIndex"> The index to start shifting at. </param>
        /// <param name="shiftAmount"> The amount to shift each index. </param>
        public void ShiftIndices(int startIndex, int shiftAmount)
        {
            Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> shiftedEntries = new();
            Dictionary<int, LinkedListNode<int>> shiftedQueuePositions = new();
            
            foreach ((int index, RecyclerScrollRectEntry<TEntryData, TKeyEntryData> recycledEntry) in _entries)
            {
                int shiftedIndex = index + (index >= startIndex ? shiftAmount : 0);
                LinkedListNode<int> shiftedIndexInQueue = _entryIndexToQueuePosition[index];

                if (shiftedIndex != index)
                {
                    recycledEntry.SetIndex(shiftedIndex);   // Shift the entry itself
                    shiftedIndexInQueue.Value = shiftedIndex;   // Shift its index in the queue
                }

                shiftedEntries[shiftedIndex] = recycledEntry;   // Shift the entry lookup
                shiftedQueuePositions[shiftedIndex] = shiftedIndexInQueue;  // Shift the entry -> queue position mapping
            }

            _entries = shiftedEntries;
            _entryIndexToQueuePosition = shiftedQueuePositions;
        }

        /// <summary>
        /// Returns the entry that has sat in the recycling pool the longest.
        /// </summary>
        /// <returns> The entry that has sat in the recycling pool the longest. </returns>
        public RecyclerScrollRectEntry<TEntryData, TKeyEntryData> GetOldestEntry()
        {
            int oldestIndex = _entryQueue.First.Value;
            return _entries[oldestIndex];
        }
    }
}