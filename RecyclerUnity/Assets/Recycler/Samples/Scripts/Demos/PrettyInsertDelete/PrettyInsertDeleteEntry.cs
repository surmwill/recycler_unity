using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Swill.Recycler
{
    /// <summary>
    /// Data for animating an entry in on insertion/deletion
    /// </summary>
    public class PrettyInsertDeleteEntry : RecyclerScrollRectEntry<PrettyInsertDeleteData, string>
    {
        [SerializeField]
        private Image _background = null;

        [SerializeField]
        private Image _backgroundGlow = null;

        [SerializeField]
        private Text _indexText = null;

        /// <summary>
        /// Whether the entry is in the process of being deleted
        /// </summary>
        public bool IsDeleting => _animateOutSequence?.IsActive() ?? false;
        
        private static readonly Color AnimateInColor = new(0x3A / 255f, 0x86 / 255f, 0xFF / 255f);
        private static readonly Color AnimateOutColor = new(0xFF / 255f, 0x00 / 255f, 0x6E / 255f);

        private const float AnimateInOutTime = 2f;

        private const int Height = 300;

        private Sequence _animateInSequence;
        private Sequence _animateOutSequence;
        
        protected override void OnBind(PrettyInsertDeleteData entryData)
        {
            _indexText.text = Index.ToString();
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(entryData.AnimateIn ? 0f : Height);
            
            _background.color = AnimateInColor;
            _backgroundGlow.fillAmount = 0f;

            if (entryData.AnimateIn)
            {
                entryData.AnimateIn = false;
                AnimateIn();
            }
        }

        protected override void OnRecycled()
        {
            _animateInSequence?.Kill(true);
            _animateOutSequence?.Kill(true);
            
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(Data.AnimateIn ? 0f : Height);
        }

        private void AnimateIn()
        {
            _backgroundGlow.fillAmount = 1f;
            _animateInSequence = DOTween.Sequence()
                .Append(DOTween.To(() => RectTransform.sizeDelta.y, newHeight => RecalculateHeight(newHeight, Data.AnimateInFixEntries), Height, AnimateInOutTime))
                .Join(_backgroundGlow.DOFillAmount(0f, AnimateInOutTime))
                .OnKill(() => _animateInSequence = null);
        }

        /// <summary>
        /// Shrinks the entry and then deletes it
        /// </summary>
        public void AnimateOutAndDelete(FixEntries fixEntries)
        {
            if (IsDeleting)
            {
                return;
            }
                
            _animateInSequence?.Kill(true);
            
            _background.color = AnimateOutColor;
            _backgroundGlow.fillAmount = 0f;
            
            _animateOutSequence = DOTween.Sequence()
                .Append(DOTween.To(() => RectTransform.sizeDelta.y, newHeight => RecalculateHeight(newHeight, fixEntries), 0f, AnimateInOutTime))
                .Join(_backgroundGlow.DOFillAmount(1f, AnimateInOutTime))
                .OnKill(() =>
                {
                    _animateOutSequence = null;
                    Recycler.RemoveAtIndex(Index);
                });
        }

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
