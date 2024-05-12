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
        (RectTransform content, RectTransform viewport) = (scrollRect.content, scrollRect.viewport);
        (WorldRect contentWorldRect, WorldRect viewportWorldRect, WorldRect childContentWorldRect) = (content.GetWorldRect(), viewport.GetWorldRect(), childContent.GetWorldRect());
        (float viewportWidth, float viewportHeight) = (viewportWorldRect.Width, viewportWorldRect.Height);

        Vector3 startHorizontalEndpoint = 
            (contentWorldRect.BotLeftCorner + 0.5f * (contentWorldRect.TopLeftCorner - contentWorldRect.BotLeftCorner)) + 
            (viewportWidth * 0.5f * content.right);
        
        Vector3 endHorizontalEndpoint =
            (contentWorldRect.BotRightCorner + 0.5f * (contentWorldRect.TopRightCorner - contentWorldRect.BotRightCorner)) -
            (viewportWidth * 0.5f * content.right);

        Vector3 startVerticalEndpoint =
            (contentWorldRect.BotLeftCorner + 0.5f * (contentWorldRect.BotRightCorner - contentWorldRect.BotLeftCorner)) +
            (viewportHeight * 0.5f * content.up);
        
        Vector3 endVerticalEndpoint = 
            (contentWorldRect.TopLeftCorner + 0.5f * (contentWorldRect.TopRightCorner - contentWorldRect.TopLeftCorner)) - 
            (viewportHeight * 0.5f * content.up);

        Vector3 viewportHorizontalLine = endHorizontalEndpoint - startHorizontalEndpoint;
        Vector3 viewportVerticalLine = endVerticalEndpoint - startVerticalEndpoint;

        Vector3 childHorizontalPosOnViewportLine = startHorizontalEndpoint + Vector3.Project(childContentWorldRect.Center - startHorizontalEndpoint, viewportHorizontalLine);
        Vector3 childVerticalPosOnViewportLine = startVerticalEndpoint + Vector3.Project(childContentWorldRect.Center - startVerticalEndpoint, viewportVerticalLine);
        
        float normalizedHorizontalDistance = Vector3Utils.InverseLerp(startHorizontalEndpoint, endHorizontalEndpoint, childHorizontalPosOnViewportLine);
        float normalizedVerticalDistance = Vector3Utils.InverseLerp(startVerticalEndpoint, endVerticalEndpoint, childVerticalPosOnViewportLine);

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
        return scrollRect.vertical && Mathf.Approximately(scrollRect.normalizedPosition.y, 1f);
    }
    
    /// <summary>
    /// Returns true if the ScrollRect is at its bottommost position (can't scroll any lower). A normalized y position of 0
    /// </summary>
    public static bool IsAtBottom(this ScrollRect scrollRect)
    {
        return scrollRect.vertical && Mathf.Abs(scrollRect.normalizedPosition.y - 0f) < 0.001f; //Mathf.Approximately(scrollRect.normalizedPosition.y, 0f);
    }
}
