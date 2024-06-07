using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecycledEntries<TEntryData>
{
    private Dictionary<int, RecyclerScrollRectEntry<TEntryData>> _entries = new();

    private LinkedList<int> _queueEntries = new();

    private Dictionary<int, LinkedListNode<int>> _entriesQueuePosition = new();

    public IReadOnlyDictionary<int, RecyclerScrollRectEntry<TEntryData>> Entries => _entries;

    public void Add(int index, RecyclerScrollRectEntry<TEntryData> entry)
    {
        _entries.Add(index, entry);
        
        LinkedListNode<int> insertionQueuePosition = _queueEntries.AddLast(index);
        _entriesQueuePosition.Add(index, insertionQueuePosition);
    }

    public void Remove(int index)
    {
        _entries.Remove(index);

        LinkedListNode<int> queuePosition = _entriesQueuePosition[index];
        _queueEntries.Remove(queuePosition);
        _entriesQueuePosition.Remove(index);
    }

    public void ShiftIndices(int startIndex, int shiftAmount)
    {
        Dictionary<int, RecyclerScrollRectEntry<TEntryData>> shiftedEntries = new Dictionary<int, RecyclerScrollRectEntry<TEntryData>>();
        Dictionary<int, LinkedListNode<int>> shiftedQueuePositions = new Dictionary<int, LinkedListNode<int>>();

        foreach ((int index, RecyclerScrollRectEntry<TEntryData> recycledEntry) in _entries)
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

    public KeyValuePair<int, RecyclerScrollRectEntry<TEntryData>> GetOldestEntry()
    {
        int oldestIndex = _queueEntries.First.Value;
        return new KeyValuePair<int, RecyclerScrollRectEntry<TEntryData>>(oldestIndex, _entries[oldestIndex]);
    }
}
