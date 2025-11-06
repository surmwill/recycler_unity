using UnityEngine;
using UnityEngine.UI;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Recycler endcap to test if we can handle auto-sized endcaps
    /// </summary>
    public class AutoSizeEndcap : RecyclerScrollRectEndcap<string, AutoSizeData>
    {
        private const int GrowShrinkAmount = 200;
        private const int MinLayoutGroupPadding = 100;

        private HorizontalOrVerticalLayoutGroup _layoutGroup;

        protected override void Awake()
        {
            base.Awake();
            _layoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
        }

        /// <summary>
        /// Increases the endcap's size through its auto-calculated layout group
        /// </summary>
        public void Grow()
        {
            if (Recycler.Orientation.IsVertical())
            {
                _layoutGroup.padding.top += GrowShrinkAmount / 2;
                _layoutGroup.padding.bottom += GrowShrinkAmount / 2;   
            }
            else
            {
                _layoutGroup.padding.left += GrowShrinkAmount / 2;
                _layoutGroup.padding.right += GrowShrinkAmount / 2;   
            }
            
            AutoRecalculateDimension();
        }

        /// <summary>
        /// Decreases the endcap's size through its auto-calculated layout group
        /// </summary>
        public void Shrink()
        {
            if (Recycler.Orientation.IsVertical())
            {
                _layoutGroup.padding.top -= GrowShrinkAmount / 2;
                _layoutGroup.padding.bottom -= GrowShrinkAmount / 2;   
                
                _layoutGroup.padding.top = Mathf.Max(_layoutGroup.padding.top, MinLayoutGroupPadding);
                _layoutGroup.padding.bottom = Mathf.Max(_layoutGroup.padding.bottom, MinLayoutGroupPadding);
            }
            else
            {
                _layoutGroup.padding.left -= GrowShrinkAmount / 2;
                _layoutGroup.padding.right -= GrowShrinkAmount / 2; 
                
                _layoutGroup.padding.left = Mathf.Max(_layoutGroup.padding.left, MinLayoutGroupPadding);
                _layoutGroup.padding.right = Mathf.Max(_layoutGroup.padding.right, MinLayoutGroupPadding);
            }
            
            AutoRecalculateDimension();
        }
    }
}
