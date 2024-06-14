using UnityEngine;

/// <summary>
/// Extensions for RectTransforms
/// </summary>
public static class RectTransformExtensions
{
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
