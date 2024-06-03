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

    private static readonly HashSet<int> EnlargeEntryIndices = new() { 41, 42 };

    private void Start()
    {
        ScrollToIndexData[] entryData = Enumerable.Repeat((ScrollToIndexData) null, InitNumEntries)
            .Select((_, i) => new ScrollToIndexData(EnlargeEntryIndices.Contains(i)))
            .ToArray();
        
        _recycler.AppendEntries(entryData);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _recycler.ScrollToIndex(ScrollToIndex);
        }
    }
}
