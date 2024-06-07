using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RecycledEntries<TEntryData>
{
    private Dictionary<int, RecyclerScrollRectEntry<TEntryData>> _entries = new();

    private LinkedList<int> _queueEntryInsertions = new();

    private Dictionary<int, LinkedListNode<int>> _entriesInsertedAt = new();

    public IReadOnlyDictionary<int, RecyclerScrollRectEntry<TEntryData>> Entries => _entries;

    public void Add(int index, RecyclerScrollRectEntry<TEntryData> entry)
    {
        _entries.Add(index, entry);
        
        LinkedListNode<int> insertionQueuePosition = _queueEntryInsertions.AddLast(index);
        _entriesInsertedAt.Add(index, insertionQueuePosition);
    }

    public void Remove(int index)
    {
        _entries.Remove(index);

        LinkedListNode<int> insertionQueuePosition = _entriesInsertedAt[index];
        _queueEntryInsertions.Remove(insertionQueuePosition);
        _entriesInsertedAt.Remove(index);
    }

    public void ShiftIndices(int startIndex, int shiftAmount)
    {
        Dictionary<int, RecyclerScrollRectEntry<TEntryData>> shiftedEntries = new Dictionary<int, RecyclerScrollRectEntry<TEntryData>>();
        Dictionary<int, LinkedListNode<int>> shiftedEntriesInsertedAt = new Dictionary<int, LinkedListNode<int>>();

        foreach ((int index, RecyclerScrollRectEntry<TEntryData> activeEntry) in _entries)
        {
            int shiftedIndex = index + (index >= startIndex ? shiftAmount : 0);
            if (shiftedIndex == index)
            {
                continue;
            }
            
            activeEntry.SetIndex(shiftedIndex);   
            shiftedEntries[shiftedIndex] = activeEntry;
            
            LinkedListNode<int> shiftedInsertionQueuePosition = _entriesInsertedAt[index];
            shiftedInsertionQueuePosition.Value = shiftedIndex;
            shiftedEntriesInsertedAt[shiftedIndex] = shiftedInsertionQueuePosition;
        }

        _entries = shiftedEntries;
        _entriesInsertedAt = shiftedEntriesInsertedAt;
    }

    public KeyValuePair<int, RecyclerScrollRectEntry<TEntryData>> GetOldestEntry()
    {
        int oldestIndex = _queueEntryInsertions.First.Value;
        return new KeyValuePair<int, RecyclerScrollRectEntry<TEntryData>>(oldestIndex, _entries[oldestIndex]);
    }
}
