using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Demo entry to test recycler deletion of entries
    /// </summary>
    public class DeleteRecyclerEntry : RecyclerScrollRectEntry<string, EmptyRecyclerData>
    {
        [SerializeField]
        private Text _indexText = null;

        private const float DeleteTime = 1.5f;

        private Sequence _deleteSequence;

        protected override void OnBind(EmptyRecyclerData _)
        {
            _indexText.text = Index.ToString();
        }

        protected override void OnRecycled()
        {
            _deleteSequence?.Kill(true);
        }

        public void ShrinkAndDelete()
        {
            if (_deleteSequence != null)
            {
                return;
            }

            float initHeightOrWidth = Recycler.Orientation.IsVertical() ? RectTransform.sizeDelta.y : RectTransform.sizeDelta.x;

            _deleteSequence = DOTween.Sequence()
                .Append(DOTween.To(() => Recycler.Orientation.IsVertical() ? RectTransform.sizeDelta.y : RectTransform.sizeDelta.x, newHeightOrWidth => RecalculateDimension(newHeightOrWidth, FixEntries.Middle), 0f, DeleteTime))
                .SetEase(Ease.OutBounce)
                .OnKill(() =>
                {
                    Recycler.RemoveAtIndex(Index, Recycler.Orientation.IsVertical() ? FixEntries.VerticalAbove : FixEntries.HorizontalLeft);
                    _deleteSequence = null;
                    RectTransform.sizeDelta = Recycler.Orientation.IsVertical() ? RectTransform.sizeDelta.WithY(initHeightOrWidth) : RectTransform.sizeDelta.WithX(initHeightOrWidth);
                });
        }

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
