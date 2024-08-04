using DG.Tweening;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Endcap that tests resizing
    /// </summary>
    public class EndcapResizeEndcap : RecyclerScrollRectEndcap<EmptyRecyclerData, string>
    {
        private const int NormalSize = 300;
        private const int GrowSize = 600;
        private const float GrowTimeSeconds = 2f;

        private Tween _resizeTween;

        public override void OnReturnedToPool()
        {
            StopResizeAndComplete();
        }

        private void StopResizeAndComplete()
        {
            _resizeTween?.Kill(true);
            _resizeTween = null;
        }

        /// <summary>
        /// Resets the endcap to its original size.
        /// </summary>
        public void ResetSizeToNormal()
        {
            StopResizeAndComplete();
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(NormalSize);
            RecalculateDimensions();
        }

        /// <summary>
        /// Grows the endcap.
        /// </summary>
        public void Grow()
        {
            _resizeTween ??= RectTransform.DOSizeDelta(RectTransform.sizeDelta.WithY(GrowSize), GrowTimeSeconds)
                .OnUpdate(() => RecalculateDimensions());
        }
    }
}
