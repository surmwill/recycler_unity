using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Extension methods for ScrollRectWithDragSensitivities.
    /// </summary>
    public static class ScrollRectWithDragSensitivityExtensions
    {
        /// <summary>
        /// Returns the normalized scroll position that centers the viewport on a child of a ScrollRectWithDragSensitivity.
        /// </summary>
        /// <param name="scrollRect"> The ScrollRectWithDragSensitivity containing the child content. </param>
        /// <param name="childContent"> The child content in the ScrollRectWithDragSensitivity to get the normalized position of. </param>
        /// <param name="normalizedPositionInChild"> The position within the child to center the viewport on. </param>
        /// <returns> The normalized scroll position that centers the viewport on a child of a ScrollRectWithDragSensitivity. </returns>
        public static float GetNormalizedVerticalPositionOfChild(
            this ScrollRectWithDragSensitivity scrollRect, 
            RectTransform childContent,
            float normalizedPositionInChild)
        {
            (RectTransform content, RectTransform viewport) = (scrollRect.content, scrollRect.viewport);
            (WorldRect contentWorldRect, WorldRect viewportWorldRect, WorldRect childContentWorldRect) = (
                content.GetWorldRect(), viewport.GetWorldRect(), childContent.GetWorldRect());

            Vector3 contentTopPosition = contentWorldRect.TopLeftCorner;
            Vector3 contentBotPosition = contentWorldRect.BotLeftCorner;

            // The viewport travels along this content line and the child lies on this content line.
            Vector3 contentBotToTop = contentTopPosition - contentBotPosition;
            Vector3 contentBotToTopNormalized = contentBotToTop.normalized;

            // The viewport's center can only travel in these positions along the content line without one if its edges hitting the end of the content.
            float viewportHeight = viewportWorldRect.Height;
            Vector3 viewportTopmostPosition = contentTopPosition - (contentBotToTopNormalized * viewportHeight / 2f);
            Vector3 viewportBotmostPosition = contentBotPosition + (contentBotToTopNormalized * viewportHeight / 2f);
            Vector3 viewportPositionsBotToTop = viewportTopmostPosition - viewportBotmostPosition;

            // Where in the child are we scrolling to (ex: its middle, top edge, bot edge, etc...).
            Vector3 positionInChild = childContentWorldRect.Center + childContent.up * ((normalizedPositionInChild - 0.5f) * childContentWorldRect.Height);
            
            // Find where the child lies along the viewport line.
            Vector3 childViewportPosition = viewportBotmostPosition + Vector3.Project(positionInChild - viewportBotmostPosition, viewportPositionsBotToTop);

            Vector3 viewportBotmostToChildPosition = childViewportPosition - viewportBotmostPosition;
            Vector3 viewportTopmostToChildPosition = childViewportPosition - viewportTopmostPosition;

            // Below where the viewport can center on 
            if (Vector3.Dot(viewportBotmostToChildPosition, -viewportPositionsBotToTop) > 0)
            {
                return -viewportBotmostToChildPosition.magnitude / viewportPositionsBotToTop.magnitude;
            }

            // Above where the viewport can center on
            if (Vector3.Dot(viewportTopmostToChildPosition, viewportPositionsBotToTop) > 0)
            {
                return 1f + viewportTopmostToChildPosition.magnitude / viewportPositionsBotToTop.magnitude;
            }

            // In range of where the viewport can center on
            float normalizedPositionInViewportBotToTop = viewportBotmostToChildPosition.magnitude / viewportPositionsBotToTop.magnitude;
            return normalizedPositionInViewportBotToTop;
        }

       
        /// <summary>
        /// Returns true if the ScrollRectWithDragSensitivity has sufficient size to be scrollable.
        /// </summary>
        /// <param name="scrollRect"> The ScrollRectWithDragSensitivity. </param>
        /// <returns> True if the ScrollRectWithDragSensitivity has sufficient size to be scrollable. </returns>
        public static bool IsScrollable(this ScrollRectWithDragSensitivity scrollRect)
        {
            Rect contentRect = scrollRect.content.rect;
            Rect viewportRect = scrollRect.viewport.rect;
            return scrollRect.vertical && contentRect.height > viewportRect.height ||
                   scrollRect.horizontal && contentRect.width > viewportRect.width;
        }
        
        /// <summary>
        /// Returns true if we are at a given normalized position in a ScrollRectWithDragSensitivity.
        /// 
        /// Note that the normalized position is based on a calculation of the position of the viewport relative to its encompassing content.
        /// When we set the normalized position the viewport moves, however there is a tiny minimum threshold of movement required.
        /// Because of these two reasons it's possible to have a normalized position very close to 0, set it to 0, and the value gets
        /// ignored because the viewport doesn't move enough. Since the viewport didn't move we will still get the same value as before:
        /// very close to 0, but not 0.
        /// 
        /// This can get confusing, for example, if we are expecting a normalized y position of 0 when we're at the bottom, but we're
        /// actually in that special case where we're very close to 0, and setting it directly to 0 won't help. To cover this case we
        /// also check if setting the normalized position to 0 results in the same normalized position as before (i.e. we are in that tiny threshold).
        /// </summary>
        /// <param name="scrollRect"> The ScrollRectWithDragSensitivity. </param>
        /// <param name="targetNormalizedPosition"> The normalized position to check if we are at in. </param>
        /// <returns> True if we are at the given normalized position. </returns>
        public static bool IsAtNormalizedPosition(this ScrollRectWithDragSensitivity scrollRect, Vector2 targetNormalizedPosition)
        {
            if (scrollRect.normalizedPosition == targetNormalizedPosition)
            {
                return true;
            }

            Vector2 prevNormalizedPosition = scrollRect.normalizedPosition;
            scrollRect.normalizedPosition = targetNormalizedPosition;
            if (scrollRect.normalizedPosition == prevNormalizedPosition)
            {
                return true;
            }

            scrollRect.normalizedPosition = prevNormalizedPosition;
            return false;
        }
    }
}
