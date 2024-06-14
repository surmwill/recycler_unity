
using UnityEngine;

/// <summary>
/// Creator of commonly used RectTransforms
/// </summary>
public static class RectTransformFactory
{
    /// <summary>
    /// Creates a RectTransform anchored to the corners of its parent with no offset (i.e. equal to size of the parent)
    /// </summary>
    public static RectTransform CreateFullRect(string name, Transform parent)
    {
        RectTransform rect = (RectTransform) new GameObject(name, typeof(RectTransform)).transform;
            
        rect.SetParent(parent);
        (rect.localScale, rect.localPosition, rect.localRotation) = (Vector3.one, Vector3.zero, Quaternion.identity);
            
        (rect.anchorMin, rect.anchorMax) = (Vector2.zero, Vector2.one);
        (rect.offsetMin, rect.offsetMax) = (Vector2.zero, Vector2.zero);

        return rect;
    }
}
