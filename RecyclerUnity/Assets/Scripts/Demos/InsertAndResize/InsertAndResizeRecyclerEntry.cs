using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InsertAndResizeRecyclerEntry : RecyclerScrollRectEntry<InsertAndResizeData, string>
{
    [SerializeField]
    private CanvasGroup _displayNumber = null;

    [SerializeField]
    private Text _numberText = null;
    
    private const int NormalSize = 250;
    private const int GrowSize = 500;
    
    private const float GrowTimeSeconds = 2f;
    private const float FadeTimeSeconds = 0.4f;
    
    private Sequence _growSequence;

    protected override void OnBindNewData(InsertAndResizeData entryData)
    {
        _displayNumber.alpha = 1f;

        if (!entryData.ShouldGrow)
        {
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(NormalSize);
        }
        else if (entryData.DidGrow)
        {
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(GrowSize);
        }
        else
        {
            entryData.DidGrow = true;
            
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(0f);
            _displayNumber.alpha = 0f;

            _growSequence = DOTween.Sequence()
                .Append(DOTween.To(
                    () => RectTransform.sizeDelta.y,
                    newHeight => RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(newHeight),
                    GrowSize,
                    GrowTimeSeconds).OnUpdate(() => RecalculateDimensions(FixEntries.Above)))
                .OnComplete(() => _displayNumber.DOFade(1f, FadeTimeSeconds));
        }
    }

    protected override void OnRebindExistingData()
    {
    }

    protected override void OnSentToRecycling()
    {
        _growSequence?.Kill(true);
    }

    private void Update()
    {
        _numberText.text = Index.ToString();
    }
}
