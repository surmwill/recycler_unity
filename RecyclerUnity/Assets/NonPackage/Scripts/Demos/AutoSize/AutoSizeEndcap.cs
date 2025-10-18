using UnityEngine;
using UnityEngine.UI;

namespace com.swill.recycler
{
    /// <summary>
    /// Recycler endcap to test if we can handle auto-sized endcaps
    /// </summary>
    public class AutoSizeEndcap : RecyclerScrollRectEndcap<AutoSizeData, string>
    {
        private const int GrowShrinkAmount = 200;
        private const int MinLayoutGroupHeight = 100;

        private VerticalLayoutGroup _layoutGroup;

        protected override void Awake()
        {
            base.Awake();
            _layoutGroup = GetComponent<VerticalLayoutGroup>();
        }

        /// <summary>
        /// Increases the endcap's size through its auto-calculated layout group
        /// </summary>
        public void Grow()
        {
            _layoutGroup.padding.top += GrowShrinkAmount / 2;
            _layoutGroup.padding.bottom += GrowShrinkAmount / 2;
            AutoRecalculateHeight();
        }

        /// <summary>
        /// Decreases the endcap's size through its auto-calculated layout group
        /// </summary>
        public void Shrink()
        {
            _layoutGroup.padding.top -= GrowShrinkAmount / 2;
            _layoutGroup.padding.bottom -= GrowShrinkAmount / 2;

            _layoutGroup.padding.top = Mathf.Max(_layoutGroup.padding.top, MinLayoutGroupHeight);
            _layoutGroup.padding.bottom = Mathf.Max(_layoutGroup.padding.bottom, MinLayoutGroupHeight);
            
            AutoRecalculateHeight();
        }
    }
}
