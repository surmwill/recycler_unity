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

        protected override void OnActiveStateChanged(RecyclerScrollRectContentState? prevState, RecyclerScrollRectContentState newState)
        {
            _colorTween?.Kill();
            
            switch (newState)
            {
                case RecyclerScrollRectContentState.Visible:
                    _colorTween = _background.DOColor(TestStateChangesRecycler.OnVisibleColor, TestStateChangesRecycler.CrossFadeTimeSeconds);
                    break;
                
                case RecyclerScrollRectContentState.InStartCache:
                    _background.color = TestStateChangesRecycler.OnStartCacheColor;
                    break;
                
                case RecyclerScrollRectContentState.InEndCache:
                    _background.color = TestStateChangesRecycler.OnEndCacheColor;
                    break;
            }
        }
    }
}
