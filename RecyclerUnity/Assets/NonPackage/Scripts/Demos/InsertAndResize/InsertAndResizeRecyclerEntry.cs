using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Recycler entry for demoing inserting and resizing entries
    /// </summary>
    public class InsertAndResizeRecyclerEntry : RecyclerScrollRectEntry<InsertAndResizeData, string>
    {
        [SerializeField]
        private CanvasGroup _displayNumber = null;

        [SerializeField]
        private Text _numberText = null;

        private const int NormalSize = 250;
        private const int GrowSize = 500;

        private const float GrowTimeSeconds = 1.5f;
        private const float FadeTimeSeconds = 0.4f;

        private Sequence _growSequence;

        protected override void OnBindNewData(InsertAndResizeData entryData)
        {
            _numberText.text = Index.ToString();
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(entryData.DidGrow ? GrowSize : NormalSize);

            if (!entryData.ShouldGrow || entryData.DidGrow)
            {
                return;
            }
            entryData.DidGrow = true;
            
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(0f);
            _displayNumber.alpha = 0f;

            _growSequence = DOTween.Sequence()
                .Append(DOTween.To(
                    () => RectTransform.sizeDelta.y,
                    newHeight => RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(newHeight),
                    GrowSize,
                    GrowTimeSeconds).OnUpdate(() => RecalculateDimensions(FixEntries.Below)))
                .Append(_displayNumber.DOFade(1f, FadeTimeSeconds));
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
}
