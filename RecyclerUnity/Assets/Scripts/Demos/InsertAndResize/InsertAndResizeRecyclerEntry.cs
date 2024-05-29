using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A recycler entry to be fed null data
/// </summary>
public class InsertAndResizeRecyclerEntry : RecyclerScrollRectEntry<InsertAndResizeData>
{
    [SerializeField]
    private CanvasGroup _displayNumber = null;
    
    private const int NormalSize = 250;
    private const int GrowSize = 500;
    
    private const float GrowTimeSeconds = 1.5f;
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
            
            _growSequence = DOTween
                .Sequence(RectTransform.DOSizeDelta(RectTransform.sizeDelta.WithY(GrowSize), GrowTimeSeconds))
                .Append(_displayNumber.DOFade(1f, FadeTimeSeconds));
        }
    }

    protected override void OnRebindExistingData()
    {
    }

    protected override void OnSentToRecycling()
    {
        _growSequence?.Kill(true);
    }
}
