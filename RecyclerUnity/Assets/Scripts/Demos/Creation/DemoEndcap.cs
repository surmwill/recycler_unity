using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Endcap for demoing
    /// </summary>
    public class DemoEndcap : RecyclerScrollRectEndcap<DemoRecyclerData, string>
    {
        private const int GrowSize = 600;
        private const float GrowTimeSeconds = 2f;

        private Tween _resizeTween = null;

        public override void OnFetchedFromRecycling()
        {

        }

        public override void OnSentToRecycling()
        {
            _resizeTween?.Kill(true);
        }

        public void Resize()
        {
            _resizeTween ??= RectTransform.DOSizeDelta(RectTransform.sizeDelta.WithY(GrowSize), GrowTimeSeconds)
                .OnUpdate(() => RecalculateDimensions());
        }
    }
}
