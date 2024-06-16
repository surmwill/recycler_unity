using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DeleteRecyclerEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
{
    [SerializeField]
    private Text _indexText = null;

    private const float DeleteTime = 1.5f;

    private Sequence _deleteSequence;

    protected override void OnBindNewData(EmptyRecyclerData _)
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
        if (_deleteSequence != null)
        {
            return;
        }

        float initHeight = RectTransform.sizeDelta.y;
        
        _deleteSequence = DOTween.Sequence()
            .Append(RectTransform.DOSizeDelta(RectTransform.sizeDelta.WithY(0f), DeleteTime)
                .SetEase(Ease.OutBounce)
                .OnUpdate(() => RecalculateDimensions(FixEntries.Mid)))
            .OnComplete(() =>
            {
                Recycler.RemoveAtIndex(Index, FixEntries.Above);
                _deleteSequence = null;
                RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(initHeight);
            });
    }

    private void Update()
    {
        _indexText.text = Index.ToString();
    }
}
