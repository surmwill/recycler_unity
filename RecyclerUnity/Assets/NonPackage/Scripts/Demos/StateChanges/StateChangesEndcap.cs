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
        
        private static readonly Color VisibleColor = new(0x00 / 255f, 0x96 / 255f, 0x89 / 255f);
        private static readonly Color NotVisibleColor = new(0xFF / 255f, 0xBD / 255f, 0x74 / 255f);
        
        private Tween _colorTween;

        protected override void OnFetchedFromPool()
        {
            _background.color = NotVisibleColor;
        }

        protected override void OnReturnedToPool()
        {
            _colorTween?.Kill();
        }

        protected override void OnVisibilityChanged(bool isVisible, bool isInitial)
        {
            _colorTween?.Kill();

            if (isVisible)
            {
                _colorTween = _background.DOColor(VisibleColor, TestStateChangesRecycler.CrossFadeTimeSeconds);
            }
            else
            {
                _colorTween = _background.DOColor(NotVisibleColor, TestStateChangesRecycler.CrossFadeTimeSeconds);
            }
        }
    }
}
