using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Maintains a dictionary of recycled entries as well as a LinkedList (acting as a queue) to track which entries have sat in recycling the longest
/// </summary>
public class RecycledEntries<TEntryData, TKeyEntryData> where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
{
    private Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> _entries = new();

    private Dictionary<int, LinkedListNode<int>> _entriesQueuePosition = new();
    
    private readonly LinkedList<int> _queueEntries = new();

    public IReadOnlyDictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> Entries => _entries;

    public void Add(int index, RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry)
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
        Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> shiftedEntries = new Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();
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

    public KeyValuePair<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> GetOldestEntry()
    {
        int oldestIndex = _queueEntries.First.Value;
        return new KeyValuePair<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>(oldestIndex, _entries[oldestIndex]);
    }
}
