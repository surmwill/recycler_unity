using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DeleteEntry : RecyclerScrollRectEntry<object>
{
    [SerializeField]
    private TMP_Text _indexText = null;

    private const float DeleteTime = 1.5f;
    
    private Sequence _deleteSequence;

    protected override void OnBindNewData(object _)
    {
    }

    protected override void OnRebindExistingData()
    {
    }

    protected override void OnSentToRecycling()
    {
        _deleteSequence?.Kill(true);
    }

    public void Delete()
    {
        if (_deleteSequence == null)
        {
            _deleteSequence = DOTween.Sequence()
                .Append(RectTransform.DOSizeDelta(RectTransform.sizeDelta.WithY(0f), DeleteTime).OnUpdate(() => RecalculateDimensions(FixEntries.Above)))
                .OnComplete(() =>
                {
                    Recycler.RemoveAt(Index, FixEntries.Above);
                    _deleteSequence = null;
                });   
        }
    }

    private void Update()
    {
        _indexText.text = Index.ToString();
    }
}
