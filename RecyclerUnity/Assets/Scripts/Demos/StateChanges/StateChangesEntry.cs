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
        
        // Colors corresponding to the different states of the entries
        private static readonly Color OnVisibleColor = new(0xFB / 255f, 0xAF / 255f, 0x00 / 255f);
        private static readonly Color OnStartCacheColor = new(0x00 / 255f, 0x7C / 255f, 0xBE / 255f);
        private static readonly Color OnEndCacheColor = new(0x00 / 255f, 0xAF / 255f, 0x54 / 255f);

        protected override void OnBindNewData(EmptyRecyclerData entryData)
        {
            _indexText.text = Index.ToString();
        }

        protected override void OnStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
        {
            if (TestStateChangesRecycler.DebugPrintStateChangesForEntryIndex == Index)
            {
                Debug.Log($"Entry {Index} changing state from {prevState} -> {newState}");
            }
            
            _colorTween?.Kill();
            
            switch (newState)
            {
                case RecyclerScrollRectContentState.ActiveVisible:
                    _colorTween = _background.DOColor(OnVisibleColor, TestStateChangesRecycler.CrossFadeTimeSeconds);
                    break;
                
                case RecyclerScrollRectContentState.ActiveInStartCache:
                    _colorTween = _background.DOColor(OnStartCacheColor, TestStateChangesRecycler.CrossFadeTimeSeconds);
                    break;
                
                case RecyclerScrollRectContentState.ActiveInEndCache:
                    _colorTween = _background.DOColor(OnEndCacheColor, TestStateChangesRecycler.CrossFadeTimeSeconds);
                    break;
            }

            // If we fetched the entry from the pool then this is its starting color
            if (prevState == RecyclerScrollRectContentState.InactiveInPool)
            {
                _colorTween?.Complete();
            }
        }
    }
}
