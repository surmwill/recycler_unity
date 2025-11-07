using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Data for animating an entry in on insertion/deletion
    /// </summary>
    public class PrettyInsertDeleteEntry : RecyclerScrollRectEntry<string, PrettyInsertDeleteData>
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
        private const int Width = 800;

        private Sequence _animateInSequence;
        private Sequence _animateOutSequence;
        
        protected override void OnBind(PrettyInsertDeleteData entryData)
        {
            _indexText.text = Index.ToString();

            if (Recycler.Orientation.IsVertical())
            {
                RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(entryData.AnimateIn ? 0f : Height);   
            }
            else
            {
                RectTransform.sizeDelta = RectTransform.sizeDelta.WithX(entryData.AnimateIn ? 0f : Width);   
            }
            
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

            if (Recycler.Orientation.IsVertical())
            {
                RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(Data.AnimateIn ? 0f : Height);   
            }
            else
            {
                RectTransform.sizeDelta = RectTransform.sizeDelta.WithX(Data.AnimateIn ? 0f : Width); 
            }
        }

        private void AnimateIn()
        {
            _backgroundGlow.fillAmount = 1f;

            float targetHeightOrWidth = Recycler.Orientation.IsVertical() ? Height : Width;
            _animateInSequence = DOTween.Sequence()
                .Append(DOTween.To(() => Recycler.Orientation.IsVertical() ? RectTransform.sizeDelta.y : RectTransform.sizeDelta.x, newHeightOrWidth => RecalculateDimension(newHeightOrWidth, Data.AnimateInFixEntries), targetHeightOrWidth, AnimateInOutTime))
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
                .Append(DOTween.To(() => Recycler.Orientation.IsVertical() ? RectTransform.sizeDelta.y : RectTransform.sizeDelta.x, newHeightOrWidth => RecalculateDimension(newHeightOrWidth, fixEntries), 0f, AnimateInOutTime))
                .Join(_backgroundGlow.DOFillAmount(1f, AnimateInOutTime))
                .OnKill(() =>
                {
                    _animateOutSequence = null;
                    Recycler.RemoveAtIndex(Index, Recycler.Orientation.IsVertical() ? FixEntries.VerticalAbove : FixEntries.HorizontalLeft);
                });
        }

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
