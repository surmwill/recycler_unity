using DG.Tweening;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Endcap that tests resizing
    /// </summary>
    public class EndcapResizeEndcap : RecyclerScrollRectEndcap<string, EmptyRecyclerData>
    {
        private const int NormalSize = 300;
        private const int GrowSize = 600;
        private const float GrowTimeSeconds = 2f;

        private Tween _resizeTween;

        protected override void OnReturnedToPool()
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
            RecalculateHeight(NormalSize);
        }

        /// <summary>
        /// Grows the endcap.
        /// </summary>
        public void Grow()
        {
            _resizeTween ??= DOTween.To(() => RectTransform.sizeDelta.y, newHeight => RecalculateHeight(newHeight), GrowSize, GrowTimeSeconds);
        }
    }
}
