using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Swill.Recycler
{
    /// <summary>
    /// Endcap for testing the changing of visible states
    /// </summary>
    public class StateChangesEndcap : RecyclerScrollRectEndcap<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Image _background = null;
        
        private Tween _colorTween;

        protected override void OnReturnedToPool()
        {
            _colorTween?.Kill();
        }

        protected override void OnVisibilityChanged(bool isVisible, bool isInitial)
        {
            _colorTween?.Kill();

            if (isVisible)
            {
                _colorTween = _background.DOColor(TestStateChangesRecycler.OnVisibleColor, TestStateChangesRecycler.CrossFadeTimeSeconds);
            }
            else
            {
                _background.color = TestStateChangesRecycler.OnNotVisibleColor;
            }
        }
    }
}
