using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Extension methods for ScrollRectWithDragSensitivities
    /// </summary>
    public static class ScrollRectWithDragSensitivityExtensions
    {
        /// <summary>
        /// Returns the normalized scroll position of one of the children that make up the ScrollRect's content
        /// </summary>
        public static float GetNormalizedVerticalPositionOfChild(this ScrollRectWithDragSensitivity scrollRect, RectTransform childContent,
            float normalizedPositionInChild)
        {
            (RectTransform content, RectTransform viewport) = (scrollRect.content, scrollRect.viewport);
            (WorldRect contentWorldRect, WorldRect viewportWorldRect, WorldRect childContentWorldRect) = (
                content.GetWorldRect(), viewport.GetWorldRect(), childContent.GetWorldRect());

            Vector3 contentTopPosition = contentWorldRect.TopLeftCorner;
            Vector3 contentBotPosition = contentWorldRect.BotLeftCorner;

            Vector3 contentBotToTop = contentTopPosition - contentBotPosition;
            Vector3 contentBotToTopNormalized = contentBotToTop.normalized;

            float viewportHeight = viewportWorldRect.Height;
            Vector3 viewportTopmostPosition = contentTopPosition - (contentBotToTopNormalized * viewportHeight / 2f);
            Vector3 viewportBotmostPosition = contentBotPosition + (contentBotToTopNormalized * viewportHeight / 2f);
            Vector3 viewportPositionsBotToTop = viewportTopmostPosition - viewportBotmostPosition;

            // Where in the child are we scrolling to (ex: its middle, top edge, bot edge, etc...)
            Vector3 positionInChild = childContentWorldRect.Center +
                                      childContent.up * ((normalizedPositionInChild - 0.5f) *
                                                         childContentWorldRect.Height);
            Vector3 childViewportPosition = viewportBotmostPosition +
                                            Vector3.Project(positionInChild - viewportBotmostPosition,
                                                viewportPositionsBotToTop);

            Vector3 viewportBotmostToChildPosition = childViewportPosition - viewportBotmostPosition;
            Vector3 viewportTopmostToChildPosition = childViewportPosition - viewportTopmostPosition;

            // Can still be seen in the lower half of viewport, but can't move the viewport down enough to center on it without hitting the end of the content 
            if (Vector3.Dot(viewportBotmostToChildPosition, viewportPositionsBotToTop) < 0)
            {
                return 0f;
            }

            // Can still be seen in the upper half of viewport, but can't move the viewport up enough to center on it without hitting the end of the content 
            if (Vector3.Dot(viewportTopmostToChildPosition, -viewportPositionsBotToTop) < 0)
            {
                return 1f;
            }

            float normalizedPositionInViewportBotToTop =
                viewportBotmostToChildPosition.magnitude / viewportPositionsBotToTop.magnitude;
            return normalizedPositionInViewportBotToTop;
        }

        /// <summary>
        /// Returns true if the ScrollRect has sufficient size to be scrollable
        /// </summary>
        public static bool IsScrollable(this ScrollRectWithDragSensitivity scrollRect)
        {
            Rect contentRect = scrollRect.content.rect;
            Rect viewportRect = scrollRect.viewport.rect;
            return scrollRect.vertical && contentRect.height > viewportRect.height ||
                   scrollRect.horizontal && contentRect.width > viewportRect.width;
        }

        /// <summary>
        /// Returns true if we are at a given normalized position.
        /// 
        /// Note that the normalized position is based on a calculation of the position of the viewport relative to its encompassing content.
        /// When we set the normalized position the viewport moves, however there is a tiny minimum threshold of movement required.
        /// Because of these two reasons it's possible to have a normalized position very close to 0, set it to 0, and the value gets
        /// ignored because the viewport doesn't move enough. Since the viewport didn't move we will still get the same value as before
        /// (very close to 0, but not 0).
        /// 
        /// This can get confusing for example, if we are expecting a normalized y position of 0 when we're at the bottom, but we're
        /// actually in that special case where we're very close to 0 - and setting it directly to 0 won't help. To cover this case we
        /// also check if setting the normalized position to 0 results in the same normalized position as before (i.e. we are in that tiny threshold).
        /// </summary>
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
