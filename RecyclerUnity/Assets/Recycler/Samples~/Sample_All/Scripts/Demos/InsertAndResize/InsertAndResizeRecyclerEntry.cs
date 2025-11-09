using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Recycler entry for demoing inserting and resizing entries
    /// </summary>
    public class InsertAndResizeRecyclerEntry : RecyclerScrollRectEntry<string, InsertAndResizeData>
    {
        [SerializeField]
        private CanvasGroup _displayNumber = null;

        [SerializeField]
        private Text _numberText = null;

        private const int NormalHeightVertical = 300;
        private const int GrowHeightVertical = 600;
        
        private const int NormalWidthHorizontal = 800;
        private const int GrowWidthHorizontal = 1200;

        private const float GrowTimeSeconds = 1.5f;
        private const float FadeTimeSeconds = 0.4f;

        private Sequence _growSequence;

        protected override void OnBind(InsertAndResizeData entryData)
        {
            _numberText.text = Index.ToString();

            if (Recycler.Orientation.IsVertical())
            {
                RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(entryData.DidGrow ? GrowHeightVertical : (entryData.ShouldGrow ? 0f : NormalHeightVertical));   
            }
            else
            {
                RectTransform.sizeDelta = RectTransform.sizeDelta.WithX(entryData.DidGrow ? GrowWidthHorizontal : (entryData.ShouldGrow ? 0f : NormalWidthHorizontal));   
            }
        }

        protected override void OnRecycled()
        {
            _growSequence?.Kill(true);
        }

        protected override void OnVisibilityChanged(bool isVisible, bool isInitial)
        {
            if (isInitial)
            {
                OnInitialVisibility(isVisible);
            }
        }

        private void OnInitialVisibility(bool initialIsVisible)
        {
            if (!Data.ShouldGrow || Data.DidGrow)
            {
                return;
            }
            Data.DidGrow = true;
            
            float growHeightOrWidth = Recycler.Orientation.IsVertical() ? GrowHeightVertical : GrowWidthHorizontal;
            FixEntries fixEntries = Recycler.Orientation.IsVertical() ? FixEntries.VerticalAbove : FixEntries.HorizontalLeft;
            
            if (!initialIsVisible)
            {
                RecalculateDimension(growHeightOrWidth, fixEntries);
                return;
            }
            
            RectTransform.sizeDelta = Recycler.Orientation.IsVertical() ? RectTransform.sizeDelta.WithY(0f) : RectTransform.sizeDelta.WithX(0f);
            _displayNumber.alpha = 0f;
            
            _growSequence = DOTween.Sequence()
                .Append(DOTween.To(() => Recycler.Orientation.IsVertical() ? RectTransform.sizeDelta.y : RectTransform.sizeDelta.x, newHeightOrWidth => RecalculateDimension(newHeightOrWidth, fixEntries), growHeightOrWidth, GrowTimeSeconds))
                .Append(_displayNumber.DOFade(1f, FadeTimeSeconds));
        }

        private void Update()
        {
            _numberText.text = Index.ToString();
        }
    }
}
