using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    public class DeleteRecyclerEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Text _indexText = null;

        private const float DeleteTime = 1.5f;

        private Sequence _deleteSequence;

        private bool _resized = false;

        protected override void OnBindNewData(EmptyRecyclerData _)
        {
            if (!_resized && Index == TestDeleteRecyclerScrollRect.DeleteAtIndex)
            {
                RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(3500f);
                _resized = true;
            }
            else
            {
                RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(250f);
            }
            
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
                .Append(RectTransform.DOSizeDelta(RectTransform.sizeDelta.WithY(0f), DeleteTime)
                    .SetEase(Ease.OutBounce)
                    .OnUpdate(() => RecalculateDimensions(FixEntries.Mid)))
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
