using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Recycler entry used to demo scrolling to an index
    /// </summary>
    public class ScrollToIndexRecyclerScrollRectEntry : RecyclerScrollRectEntry<ScrollToIndexData, string>
    {
        [SerializeField]
        private Text _numberText = null;

        private const float GrowShrinkTime = 2.5f;

        private const int NormalSize = 1200;
        private const int GrowSize = 2000;
        private const int ShrinkSize = 400;

        private Sequence _sequence;

        protected override void OnBindNewData(ScrollToIndexData entryData)
        {
            _numberText.text = Index.ToString();
        }

        protected override void OnSentToRecycling()
        {
            _sequence?.Kill();
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(NormalSize);
        }

        /// <summary>
        /// Grows the entry.
        /// </summary>
        public void Grow(FixEntries fixEntries)
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence()
                .Append(DOTween.To(() => RectTransform.sizeDelta.y, newHeight => RecalculateHeight(newHeight, fixEntries), GrowSize, GrowShrinkTime));
        }

        /// <summary>
        /// Shrinks the entry.
        /// </summary>
        public void Shrink(FixEntries fixEntries)
        {
            _sequence?.Kill();
            _sequence = DOTween.Sequence()
                .Append(DOTween.To(() => RectTransform.sizeDelta.y, newHeight => RecalculateHeight(newHeight, fixEntries), ShrinkSize, GrowShrinkTime));
        }

        private void Update()
        {
            _numberText.text = Index.ToString();
        }
    }
}
