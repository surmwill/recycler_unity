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
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(entryData.DidGrow ? GrowSize : (entryData.ShouldGrow ? 0f : NormalSize));
        }

        protected override void OnSentToRecycling()
        {
            _growSequence?.Kill(true);
        }

        protected override void OnStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
        {
            if (prevState == RecyclerScrollRectContentState.InactiveInPool)
            {
                OnFirstState(newState);
            }
        }

        private void OnFirstState(RecyclerScrollRectContentState firstState)
        {
            if (!Data.ShouldGrow || Data.DidGrow)
            {
                return;
            }
            Data.DidGrow = true;

            if (firstState != RecyclerScrollRectContentState.ActiveVisible)
            {
                RecalculateHeight(GrowSize, FixEntries.Below);
                return;
            }
            
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(0f);
            _displayNumber.alpha = 0f;
            _growSequence = DOTween.Sequence()
                .Append(DOTween.To(() => RectTransform.sizeDelta.y,
                        newHeight => RecalculateHeight(newHeight, FixEntries.Below), GrowSize, GrowTimeSeconds))
                .Append(_displayNumber.DOFade(1f, FadeTimeSeconds));
        }

        private void Update()
        {
            _numberText.text = Index.ToString();
        }
    }
}
