using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Endcap that tests resizing
    /// </summary>
    public class EndcapResizeEndcap : RecyclerScrollRectEndcap<EmptyRecyclerData, string>
    {
        private const int GrowSize = 600;
        private const float GrowTimeSeconds = 2f;

        private Tween _resizeTween = null;

        public override void OnReturnedToPool()
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
