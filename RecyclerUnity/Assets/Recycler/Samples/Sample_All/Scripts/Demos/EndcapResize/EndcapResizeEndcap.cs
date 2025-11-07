using DG.Tweening;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Endcap that tests resizing
    /// </summary>
    public class EndcapResizeEndcap : RecyclerScrollRectEndcap<string, EmptyRecyclerData>
    {
        private const int NormalSize = 400;
        private const int GrowSize = 800;
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
            RecalculateDimension(NormalSize);
        }

        /// <summary>
        /// Grows the endcap.
        /// </summary>
        public void Grow()
        {
            _resizeTween ??= DOTween.To(() => Recycler.Orientation.IsVertical() ? RectTransform.sizeDelta.y : RectTransform.sizeDelta.x, newHeightOrWidth => RecalculateDimension(newHeightOrWidth), GrowSize, GrowTimeSeconds);
        }
    }
}
