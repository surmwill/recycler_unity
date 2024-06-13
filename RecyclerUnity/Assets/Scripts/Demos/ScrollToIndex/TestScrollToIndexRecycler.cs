using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Demos scrolling to an index in a recycler
/// </summary>
public class TestScrollToIndexRecycler : MonoBehaviour
{
    [SerializeField]
    private ScrollToIndexRecyclerScrollRect _recycler = null;
    
    private const int InitNumEntries = 100;
    private const int ScrollToIndex = 45;

    private static readonly int[] EnlargeEntryIndices = { 41, 42 };

    private void Start()
    {
        _recycler.AppendEntries(CreateEntryData(InitNumEntries, EnlargeEntryIndices));
    }

    private void Update()
    {
        // Test scrolling
        if (Input.GetKeyDown(KeyCode.A))
        {
            _recycler.ScrollToIndex(ScrollToIndex, scrollSpeedViewportsPerSecond:1f);
        }
        // Test deletion while scrolling
        else if (Input.GetKeyDown(KeyCode.D))
        {
            _recycler.RemoveRange(10, 10, FixEntries.Above);
        }
        // Test insertion while scrolling
        else if (Input.GetKeyDown(KeyCode.S))
        {
            _recycler.InsertRange(10, CreateEntryData(10), FixEntries.Above);
        }
        // Test immediate scrolling
        else if (Input.GetKeyDown(KeyCode.X))
        {
            _recycler.ScrollToIndex(ScrollToIndex, isImmediate:true);
        }
        // Test cancelling scrolling
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            _recycler.StopScrolling();
        }
    }

    private ScrollToIndexData[] CreateEntryData(int numEntries, IEnumerable<int> enlargeIndices = null)
    {
        HashSet<int> enlarge = new HashSet<int>(enlargeIndices ?? Array.Empty<int>());
        return Enumerable.Repeat((ScrollToIndexData) null, numEntries)
            .Select((_, i) => new ScrollToIndexData(enlarge.Contains(i)))
            .ToArray();

    }
}
