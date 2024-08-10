using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Demo entry to test recycler deletion of entries
    /// </summary>
    public class DeleteRecyclerEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Text _indexText = null;

        private const float DeleteTime = 1.5f;

        private Sequence _deleteSequence;

        protected override void OnBindNewData(EmptyRecyclerData _)
        {
            _indexText.text = Index.ToString();
        }

        protected override void OnSentToRecycling()
        {
            _deleteSequence?.Kill(true);
            
        }

        public void ShrinkAndDelete()
        {
            if (_deleteSequence != null)
            {
                return;
            }

            float initHeight = RectTransform.sizeDelta.y;

            _deleteSequence = DOTween.Sequence()
                .Append(DOTween.To(() => RectTransform.sizeDelta.y, newHeight => RecalculateHeight(newHeight, FixEntries.Mid), 0f, DeleteTime))
                .SetEase(Ease.OutBounce)
                .OnKill(() =>
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
}
