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
        private Text _indexText = null;
        
        private static readonly Color AnimateInColor = new(0x3A, 0x86, 0xFF);
        private static readonly Color AnimateOutColor = new(0xFF, 0x00, 0x6E);

        private const float AnimateInTime = 2f;

        private const int Height = 300;

        private Sequence _sequence;
        
        protected override void OnBindNewData(PrettyInsertDeleteData entryData)
        {
            _background.color = AnimateInColor;
            _indexText.text = Index.ToString();
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(entryData.AnimateIn ? 0f : Height);
        }

        protected override void OnSentToRecycling()
        {
        }

        private void AnimateIn()
        {
            _sequence = DOTween.Sequence()
                .Append(RectTransform.DOSizeDelta(RectTransform.sizeDelta.WithY(Height), AnimateInTime))
                .OnUpdate(() => RecalculateDimensions(FixEntries.Below))
                .OnKill(() =>
                {
                    RecalculateDimensions(FixEntries.Below);
                    Data.AnimateIn = false;
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
            
            if (newState == RecyclerScrollRectContentState.ActiveInStartCache || 
                newState == RecyclerScrollRectContentState.ActiveInEndCache || 
                newState == RecyclerScrollRectContentState.InactiveInPool)
            {
                _sequence.Kill(true);
            }
        }

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
