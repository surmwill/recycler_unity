
namespace Swill.Recycler
{
    /// <summary>
    /// Extensions for the RecyclerScrollRectOrientation enum
    /// </summary>
    public static class RecyclerScrollRectOrientationExtensions
    {
        /// <summary>
        /// Returns true if we are running the recycler in a vertical orientation
        /// </summary>
        /// <returns> True if we are running the recycler in a vertical orientation </returns>
        public static bool IsVertical(this RecyclerScrollRectOrientation orientation)
        {
            return orientation == RecyclerScrollRectOrientation.TopToBottom || orientation == RecyclerScrollRectOrientation.BottomToTop;
        }
        
        /// <summary>
        /// Returns true if we are running the recycler in a horizontal orientation
        /// </summary>
        /// <returns> True if we are running the recycler in a horizontal orientation </returns>
        public static bool IsHorizontal(this RecyclerScrollRectOrientation orientation)
        {
            return orientation == RecyclerScrollRectOrientation.LeftToRight || orientation == RecyclerScrollRectOrientation.RightToLeft;
        }
    }
}
