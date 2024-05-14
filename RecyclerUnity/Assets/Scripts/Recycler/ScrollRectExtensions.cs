using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extension methods for ScrollRects
/// </summary>
public static class ScrollRectExtensions
{
    /// <summary>
    /// Returns the number of viewports scrolled per second
    /// </summary>
    public static Vector2 GetViewportVelocity(this ScrollRect scrollRect)
    {
        Vector2 velocity = scrollRect.content.TransformVector(scrollRect.velocity);
        WorldRect viewportRect = scrollRect.viewport.GetWorldRect();
        return new Vector2(velocity.x / viewportRect.Width, velocity.y / viewportRect.Height);
    }

    /// <summary>
    /// Returns the normalized scroll position of one of the children that make up the ScrollRect's content
    /// </summary>
    public static Vector2 GetNormalizedScrollPositionOfChild(this ScrollRect scrollRect, RectTransform childContent)
    {
        Vector2 normalizedCenter = new Vector2(0.5f, 0.5f);
        return GetNormalizedScrollPositionOfChild(scrollRect, childContent, normalizedCenter);
    }
    
    /// <summary>
    /// Returns the normalized scroll position of one of the children that make up the ScrollRect's content
    /// </summary>
    public static Vector2 GetNormalizedScrollPositionOfChild(this ScrollRect scrollRect, RectTransform childContent, Vector2 normalizedPositionInChild)
    {
        (RectTransform content, RectTransform viewport) = (scrollRect.content, scrollRect.viewport);
        (WorldRect contentWorldRect, WorldRect viewportWorldRect, WorldRect childContentWorldRect) = (content.GetWorldRect(), viewport.GetWorldRect(), childContent.GetWorldRect());
        (float viewportWidth, float viewportHeight) = (viewportWorldRect.Width, viewportWorldRect.Height);

        // The furthest left the viewport can scroll before bumping into the side
        Vector3 leftmostViewportPosition = 
            (contentWorldRect.BotLeftCorner + 0.5f * (contentWorldRect.TopLeftCorner - contentWorldRect.BotLeftCorner)) + 
            (viewportWidth * 0.5f * content.right);
        
        // The furthest right the viewport can scroll before bumping into the side
        Vector3 rightMostViewportPosition =
            (contentWorldRect.BotRightCorner + 0.5f * (contentWorldRect.TopRightCorner - contentWorldRect.BotRightCorner)) -
            (viewportWidth * 0.5f * content.right);

        // The furthest down the viewport can scroll before bumping into the bottom
        Vector3 bottommostViewportPosition =
            (contentWorldRect.BotLeftCorner + 0.5f * (contentWorldRect.BotRightCorner - contentWorldRect.BotLeftCorner)) +
            (viewportHeight * 0.5f * content.up);
        
        // The furthest up the viewport can scroll before bumping into the top
        Vector3 topmostViewportPosition = 
            (contentWorldRect.TopLeftCorner + 0.5f * (contentWorldRect.TopRightCorner - contentWorldRect.TopLeftCorner)) - 
            (viewportHeight * 0.5f * content.up);

        // These lines are the values that the viewport can take on
        Vector3 viewportHorizontalLine = rightMostViewportPosition - leftmostViewportPosition;
        Vector3 viewportVerticalLine = topmostViewportPosition - bottommostViewportPosition;

        // The point in the child we are scrolling to
        Vector3 scrollToPositionInChild = childContentWorldRect.BotLeftCorner + 
                                  childContentWorldRect.Right * (normalizedPositionInChild.x * childContentWorldRect.Width) +
                                  childContentWorldRect.Up * (normalizedPositionInChild.y * childContentWorldRect.Height);

        Vector3 childHorizontalPosOnViewportLine = leftmostViewportPosition + Vector3.Project(scrollToPositionInChild - leftmostViewportPosition, viewportHorizontalLine);
        Vector3 childVerticalPosOnViewportLine = bottommostViewportPosition + Vector3.Project(scrollToPositionInChild - bottommostViewportPosition, viewportVerticalLine);
        
        float normalizedHorizontalDistance = viewportHorizontalLine == Vector3.zero ? 0f : 
            Vector3Utils.InverseLerp(leftmostViewportPosition, rightMostViewportPosition, childHorizontalPosOnViewportLine);
        
        float normalizedVerticalDistance = viewportVerticalLine == Vector3.zero ? 0f :
            Vector3Utils.InverseLerp(bottommostViewportPosition, topmostViewportPosition, childVerticalPosOnViewportLine);

        return new Vector2(normalizedHorizontalDistance, normalizedVerticalDistance);
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
