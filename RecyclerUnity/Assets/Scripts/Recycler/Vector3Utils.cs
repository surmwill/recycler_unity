using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Helpful functions for working with Vector3s
/// </summary>
public static class Vector3Utils
{
    /// <summary>
    /// Returns a value in [0,1] corresponding to where the given point falls in-between points a and b.
    /// Values that fall beyond a and b on the line are clamped to [0,1]
    /// </summary>
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        (Vector3 ab, Vector3 aValue) = (b - a, value - a);

        Assert.IsTrue(ab != Vector3.zero, "Cannot inverse lerp with a line of 0 length");
        Assert.IsTrue(Vector3.Cross(ab, aValue) == Vector3.zero, "The given point is required to lie on the same line as the two others");
        
        return Mathf.Clamp01(Vector3.Dot(aValue, ab) / Vector3.Dot(ab, ab));
    }
}
