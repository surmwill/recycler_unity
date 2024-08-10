using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Recycler endcap to test if we can handle auto-sized endcaps
    /// </summary>
    public class AutoSizeEndcap : RecyclerScrollRectEndcap<AutoSizeData, string>
    {
        private const int GrowShrinkAmount = 200;

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
            RecalculateHeight(null);
        }

        /// <summary>
        /// Decreases the endcap's size throught its auto-caclulated layout group
        /// </summary>
        public void Shrink()
        {
            _layoutGroup.padding.top -= GrowShrinkAmount / 2;
            _layoutGroup.padding.bottom -= GrowShrinkAmount / 2;

            _layoutGroup.padding.top = Mathf.Max(_layoutGroup.padding.top, 100);
            _layoutGroup.padding.bottom = Mathf.Max(_layoutGroup.padding.bottom, 100);
            
            RecalculateHeight(null);
        }
    }
}
