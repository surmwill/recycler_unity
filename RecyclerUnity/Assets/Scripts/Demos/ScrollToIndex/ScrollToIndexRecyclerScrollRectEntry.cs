using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Recycler entry used to demo scrolling to an index
/// </summary>
public class ScrollToIndexRecyclerScrollRectEntry : RecyclerScrollRectEntry<ScrollToIndexData, string>
{
    [SerializeField]
    private Text _numberText = null;
    
    private const int NormalSize = 250;
    private const int GrowSize = 500;

    protected override void OnBindNewData(ScrollToIndexData entryData)
    {
        RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(entryData.ShouldResize ? GrowSize : NormalSize);
    }

    protected override void OnRebindExistingData()
    {
    }

    protected override void OnSentToRecycling()
    {
    }

    private void Update()
    {
        _numberText.text = Index.ToString();
    }
}
