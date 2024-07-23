using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests clearing and adding entries to a recycler, one-by-one
    /// </summary>
    public class StateChangesEndcap : RecyclerScrollRectEndcap<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Image _background = null;
        
        private static readonly Color OnVisibleColor = new(0x00 / 255f, 0x96 / 255f, 0x89 / 255f);
        private static readonly Color OnEndCacheColor = new(0xFF / 255f, 0xBD / 255f, 0x74 / 255f);
        
        private Tween _colorTween;
        
        protected override void OnStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
        {
            Debug.Log($"Endcap changing state from {prevState} -> {newState}");
            
            _colorTween?.Kill();
            
            switch (newState)
            {
                case RecyclerScrollRectContentState.ActiveVisible:
                    _colorTween = _background.DOColor(OnVisibleColor, TestStateChangesRecycler.CrossFadeTimeSeconds);
                    break;

                case RecyclerScrollRectContentState.ActiveInEndCache:
                    _colorTween = _background.DOColor(OnEndCacheColor, TestStateChangesRecycler.CrossFadeTimeSeconds);
                    break;
            }

            // If we fetched the endcap from its pool then this is its starting color
            if (prevState == RecyclerScrollRectContentState.InactiveInPool)
            {
                _colorTween.Complete();
            }
        }
    }
}
