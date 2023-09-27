using UnityEngine;

/// <summary>
/// Extensions for RectTransforms
/// </summary>
public static class RectTransformExtensions
{
    /// <summary>
    /// Returns true if the any part of the RectTransform overlaps any part of the other RectTransform
    /// </summary>
    public static bool Overlaps(this RectTransform r, RectTransform other)
    {
        (WorldRect worldRect, WorldRect otherWorldRect) = (r.GetWorldRect(), other.GetWorldRect());
        return worldRect.Contains(otherWorldRect.BotLeftCorner) || worldRect.Contains(otherWorldRect.TopLeftCorner) || 
               worldRect.Contains(otherWorldRect.TopRightCorner) || worldRect.Contains(otherWorldRect.BotRightCorner) ||
               otherWorldRect.Contains(worldRect.BotLeftCorner) || otherWorldRect.Contains(worldRect.TopLeftCorner) ||
               otherWorldRect.Contains(worldRect.TopRightCorner) || otherWorldRect.Contains(worldRect.BotRightCorner);
    }
    
    /// <summary>
    /// Set pivot without changing the position of the element
    /// </summary>
    public static void SetPivotWithoutMoving(this RectTransform rectTransform, Vector2 pivot)
    {
        Vector2 offset = pivot - rectTransform.pivot;
        offset.Scale(rectTransform.rect.size);
        Vector3 worldPos = rectTransform.position + rectTransform.TransformVector(offset);
        rectTransform.pivot = pivot;
        rectTransform.position = worldPos;                 
    }

    /// <summary>
    /// Returns a rectangle of the 4 world vertices that make up the RectTransform
    /// (Note the built in RectTransform.rect is defined locally and not relative to the world)
    /// </summary>
    public static WorldRect GetWorldRect(this RectTransform rectTransform)
    {
        return new WorldRect(rectTransform); 
    }
}
