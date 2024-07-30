using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
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
        public bool IsDeleteing => _animateOutSequence?.IsActive() ?? false;
        
        private static readonly Color AnimateInColor = new(0x3A / 255f, 0x86 / 255f, 0xFF / 255f);
        private static readonly Color AnimateOutColor = new(0xFF / 255f, 0x00 / 255f, 0x6E / 255f);

        private const float AnimateInOutTime = 2f;

        private const int Height = 300;

        private Sequence _animateInSequence;
        private Sequence _animateOutSequence;
        
        protected override void OnBindNewData(PrettyInsertDeleteData entryData)
        {
            _indexText.text = Index.ToString();
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(entryData.AnimateIn ? 0f : Height);
            
            _background.color = AnimateInColor;
            _backgroundGlow.fillAmount = 0f;
        }

        protected override void OnSentToRecycling()
        {
            _animateOutSequence?.Kill(true);
        }

        private void AnimateIn()
        {
            _backgroundGlow.fillAmount = 1f;
            
            _animateInSequence = DOTween.Sequence()
                .Append(RectTransform.DOSizeDelta(RectTransform.sizeDelta.WithY(Height), AnimateInOutTime))
                .Join(_backgroundGlow.DOFillAmount(0f, AnimateInOutTime))
                .OnUpdate(() => RecalculateDimensions(Data.AnimateInFixEntries))
                .OnKill(() =>
                {
                    RecalculateDimensions(FixEntries.Below);
                    Data.AnimateIn = false;
                });
        }

        /// <summary>
        /// Shrinks the entry and then deletes it
        /// </summary>
        public void AnimateOutAndDelete(FixEntries fixEntries)
        {
            if (_animateOutSequence?.IsActive() ?? false)
            {
                return;
            }
            
            _background.color = AnimateOutColor;
            _backgroundGlow.fillAmount = 0f;
            
            _animateOutSequence = DOTween.Sequence()
                .Append(RectTransform.DOSizeDelta(RectTransform.sizeDelta.WithY(0f), AnimateInOutTime))
                .Join(_backgroundGlow.DOFillAmount(1f, AnimateInOutTime))
                .OnUpdate(() => RecalculateDimensions(fixEntries))
                .OnKill(() =>
                {
                    RecalculateDimensions(fixEntries);
                    Recycler.RemoveAtIndex(Index);
                });
        }

        protected override void OnStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
        {
            if (!Data.AnimateIn)
            {
                return;
            }

            if (prevState == RecyclerScrollRectContentState.InactiveInPool)
            {
                AnimateIn();
            }
            
            if ((!_animateOutSequence?.IsActive() ?? true) && 
                (newState == RecyclerScrollRectContentState.ActiveInStartCache || 
                 newState == RecyclerScrollRectContentState.ActiveInEndCache || 
                 newState == RecyclerScrollRectContentState.InactiveInPool))
            {
                _animateInSequence.Kill(true);
            }
        }

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
