using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A recycler entry displaying a string (used for testing the recycler with simple data)
/// </summary>
public class StringScrollRectEntry : RecyclerScrollRectEntry<string>
{
    [SerializeField]
    private TMP_Text _text = null;

    [SerializeField]
    private LayoutElement _spaceTop = null;
    
    [SerializeField]
    private LayoutElement _spaceBottom = null;

    private const int TargetEntryIndex = 3;
    private const float SpaceIncrease = 300;

    private Tween _growShrinkTween;

    private static readonly Dictionary<int, float> SpacePerEntry = new();

    protected override void OnBindNewData(string entryData)
    {
        SpacePerEntry.TryGetValue(Index, out float lastSpace);
        (_spaceBottom.preferredHeight, _spaceTop.preferredHeight) = (lastSpace, lastSpace);
        _text.text = entryData;
    }

    protected override void OnRebindExistingData()
    {
        // Do nothing
    }
    
    protected override void OnSentToRecycling()
    {
        _growShrinkTween?.Kill(true);
    }

    protected void Update()
    {
        if (Index != TargetEntryIndex)
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            GrowShrink(true);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            GrowShrink(false);
        }
    }

    private void GrowShrink(bool shouldGrow)
    {
        _growShrinkTween?.Kill(true);
        RecalculateDimensions(FixEntries.Below);

        float nextSpace = _spaceTop.preferredHeight + SpaceIncrease * (shouldGrow ? 1 : -1);
        SpacePerEntry[Index] = nextSpace;

        _growShrinkTween = DOTween.To(
            () => _spaceTop.preferredHeight,
            space =>
            {
                (_spaceTop.preferredHeight, _spaceBottom.preferredHeight) = (space, space);
                RecalculateDimensions(FixEntries.Below);
            },
            nextSpace,
            2f);
    }
}