using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Entry for testing clearing and adding entries to a recycler, one-by-one
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

        protected override void OnStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
        {
            _colorTween?.Kill();
            
            switch (newState)
            {
                case RecyclerScrollRectContentState.ActiveVisible:
                    _colorTween = _background.DOColor(TestStateChangesRecycler.OnVisibleColor, TestStateChangesRecycler.CrossFadeTimeSeconds);
                    break;
                
                case RecyclerScrollRectContentState.ActiveInStartCache:
                    _background.color = TestStateChangesRecycler.OnStartCacheColor;
                    break;
                
                case RecyclerScrollRectContentState.ActiveInEndCache:
                    _background.color = TestStateChangesRecycler.OnEndCacheColor;
                    break;
            }
        }
    }
}
