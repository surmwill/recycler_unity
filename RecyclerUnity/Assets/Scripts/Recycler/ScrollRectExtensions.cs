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
        
        float normalizedHorizontalDistance = Vector3Utils.InverseLerp(leftmostViewportPosition, rightMostViewportPosition, childHorizontalPosOnViewportLine);
        float normalizedVerticalDistance = Vector3Utils.InverseLerp(bottommostViewportPosition, topmostViewportPosition, childVerticalPosOnViewportLine);

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
    /// Returns true if the ScrollRect is at its topmost position (can't scroll any higher). A normalized y position of 1
    /// </summary>
    public static bool IsAtTop(this ScrollRect scrollRect)
    {
        if (!scrollRect.vertical)
        {
            return false;
        }

        if (Mathf.Approximately(scrollRect.normalizedPosition.y, 1f))
        {
            return true;
        }

        // The normalized position is based on the position of the viewport relative to its container content. When we
        // set the normalized position the viewport moves, and in some cases, if the change in position is so small, the 
        // viewport doesn't move at all. This leaves us with the initial unchanged normalized position. Check if we are in this case
        // (so close to 1)
        float prevNormalizedY = scrollRect.normalizedPosition.y;
        scrollRect.normalizedPosition = scrollRect.normalizedPosition.WithY(1f);
        if (Mathf.Approximately(prevNormalizedY, scrollRect.normalizedPosition.y))
        {
            return true;
        }

        scrollRect.normalizedPosition = scrollRect.normalizedPosition.WithY(prevNormalizedY);
        return false;
    }
    
    /// <summary>
    /// Returns true if the ScrollRect is at its bottommost position (can't scroll any lower). A normalized y position of 0
    /// </summary>
    public static bool IsAtBottom(this ScrollRect scrollRect)
    {
        if (!scrollRect.vertical)
        {
            return false;
        }

        if (Mathf.Approximately(scrollRect.normalizedPosition.y, 0f))
        {
            return true;
        }

        // The normalized position is based on the position of the viewport relative to its container content. When we
        // set the normalized position the viewport moves, and in some cases, if the change in position is so small, the 
        // viewport doesn't move at all. This leaves us with the initial unchanged normalized position. Check if we are in this case
        // (so close to 0)
        float prevNormalizedY = scrollRect.normalizedPosition.y;
        scrollRect.normalizedPosition = scrollRect.normalizedPosition.WithY(0f);
        if (Mathf.Approximately(prevNormalizedY, scrollRect.normalizedPosition.y))
        {
            return true;
        }

        scrollRect.normalizedPosition = scrollRect.normalizedPosition.WithY(prevNormalizedY);
        return false;
    }
}
