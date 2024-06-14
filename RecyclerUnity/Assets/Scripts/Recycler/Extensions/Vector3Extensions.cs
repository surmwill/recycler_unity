using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Extensions for Vector3s
/// </summary>
public static class Vector3Extensions
{
    /// <summary>
    /// Returns the vector with a new x value
    /// </summary>
    public static Vector3 WithX(this Vector3 vec, float x)
    {
        return new Vector3(x, vec.y, vec.z);
    }
    
    /// <summary>
    /// Returns the vector with a new y value
    /// </summary>
    public static Vector3 WithY(this Vector3 vec, float y)
    {
        return new Vector3(vec.x, y, vec.z);
    }
    
    /// <summary>
    /// Returns the vector with a new z value
    /// </summary>
    public static Vector3 WithZ(this Vector3 vec, float z)
    {
        return new Vector3(vec.x, vec.y, z);
    }

    /// <summary>
    /// Normal printing of vectors rounds the values - this doesn't
    /// </summary>
    public static string PrecisePrint(this Vector3 vec)
    {
        return $"({vec.x},{vec.y},{vec.z})";
    }
}
