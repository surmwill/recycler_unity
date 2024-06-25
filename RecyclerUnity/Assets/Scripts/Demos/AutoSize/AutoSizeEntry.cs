using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Recycler entry to test if we can handle auto-sized content
/// </summary>
public class AutoSizeEntry : RecyclerScrollRectEntry<AutoSizeData, string>
{
    [SerializeField]
    private Text _titleText = null;

    [SerializeField]
    private Text _linesText = null;

    private const int MinNumLines = 1;
    private const int MaxNumLines = 6;

    protected override void OnBindNewData(AutoSizeData entryData)
    {
        _titleText.text = $"Randomly Generated {entryData.NumLines} Lines";

        _linesText.text = Enumerable.Range(0, Random.Range(MinNumLines, MaxNumLines + 1))
            .Aggregate(string.Empty, (s, i) => s + $"Line: {i + 1}\n")
            .Trim();
    }

    protected override void OnRebindExistingData()
    {
    }

    protected override void OnSentToRecycling()
    {
    }
}
