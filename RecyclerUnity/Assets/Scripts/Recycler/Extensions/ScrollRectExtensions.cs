using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extension methods for ScrollRects
/// </summary>
public static class ScrollRectExtensions
{
    /// <summary>
    /// Returns the normalized scroll position of one of the children that make up the ScrollRect's content
    /// </summary>
    public static float GetNormalizedVerticalPositionOfChild(this ScrollRect scrollRect, RectTransform childContent, float normalizedPositionInChild)
    {
        (RectTransform content, RectTransform viewport) = (scrollRect.content, scrollRect.viewport);
        (WorldRect contentWorldRect, WorldRect viewportWorldRect, WorldRect childContentWorldRect) = (content.GetWorldRect(), viewport.GetWorldRect(), childContent.GetWorldRect());

        Vector3 contentTopPosition = contentWorldRect.TopLeftCorner;
        Vector3 contentBotPosition = contentWorldRect.BotLeftCorner;
        Vector3 contentBotToTop = contentTopPosition - contentBotPosition;

        (Vector3 contentBotToTopNormalized, float viewportHeight) = (contentBotToTop.normalized, viewportWorldRect.Height);
        Vector3 viewportTopPosition = contentTopPosition - (contentBotToTopNormalized * viewportHeight / 2f);
        Vector3 viewportBotPosition = contentBotPosition + (contentBotToTopNormalized * viewportHeight / 2f);
        Vector3 viewportBotToTop = viewportTopPosition - viewportBotPosition;
        
        // Where in the child are we scrolling to (ex: its middle, top edge, bot edge, etc...)
        Vector3 childPosition = viewportBotPosition + Vector3.Project(
            childContentWorldRect.Center + childContent.up * ((normalizedPositionInChild - 0.5f) * childContentWorldRect.Height) - viewportBotPosition, viewportBotToTop);
        
        Vector3 viewportBotToChildPosition = childPosition - viewportBotPosition;
        Vector3 viewportTopToChildPosition = childPosition - viewportTopPosition;
        
        if (Vector3.Dot(viewportBotToChildPosition, viewportBotToTop) < 0)
        {
            return 0f;
        }

        if (Vector3.Dot(viewportTopToChildPosition, -viewportBotToTop) < 0)
        {
            return 1f;
        }

        float normalizedPositionInViewportBotToTop = viewportBotToChildPosition.magnitude / viewportBotToTop.magnitude;
        return normalizedPositionInViewportBotToTop;
    }

    /// <summary>
    /// Returns true if the ScrollRect has sufficient size to be scrollable
    /// </summary>
    public static bool IsScrollable(this ScrollRect scrollRect)
    {
        Rect contentRect = scrollRect.content.rect;
        Rect viewportRect = scrollRect.viewport.rect;
        return scrollRect.vertical && contentRect.height > viewportRect.height ||
               scrollRect.horizontal && contentRect.width > viewportRect.width;
    }

    /// <summary>
    /// Returns true if the ScrollRect is at its topmost position (can't scroll any higher)
    /// </summary>
    public static bool IsAtTop(this ScrollRect scrollRect)
    {
        return IsAtNormalizedPosition(scrollRect, scrollRect.normalizedPosition.WithY(1f));
    }
    
    /// <summary>
    /// Returns true if the ScrollRect is at its bottommost position (can't scroll any lower)
    /// </summary>
    public static bool IsAtBottom(this ScrollRect scrollRect)
    {
        return IsAtNormalizedPosition(scrollRect, scrollRect.normalizedPosition.WithY(0f));
    }

    /// <summary>
    /// Returns true if we are at a given normalized position.
    /// 
    /// Note that the normalized position is based on a calculation of the position of the viewport within its encompassing content.
    /// When we set the normalized position the viewport moves, however there is a tiny minimum threshold of movement required.
    /// Because of these two reasons it's possible to have a normalized position very close to 0, set it to 0, and the value gets
    /// ignored because the viewport doesn't move enough. Since the viewport didn't move we will still *get* the same value as before
    /// (very close to 0, but not 0).
    /// 
    /// This can get confusing for example, if we are expecting a normalized y position of 0 when we're at the bottom, but we're
    /// actually in that special case where we're very close to 0 (and setting it directly to 0 won't help). To cover this case we
    /// also check if setting the normalized position to 0 results in the same normalized position as before (i.e. we are in that tiny threshold).
    /// </summary>
    public static bool IsAtNormalizedPosition(this ScrollRect scrollRect, Vector2 targetNormalizedPosition)
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
