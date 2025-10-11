using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Entry for testing the changing of visible to non-visible states 
    /// </summary>
    public class StateChangesEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Text _indexText = null;

        [SerializeField]
        private Image _background = null;

        private Tween _colorTween;

        protected override void OnBindNewData(EmptyRecyclerData entryData)
        {
            _indexText.text = Index.ToString();
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
