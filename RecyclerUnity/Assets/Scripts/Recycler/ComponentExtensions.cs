using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extension methods for Componenet
/// </summary>
public static class ComponentExtensions
{
    /// <summary>
    /// Returns true if the component is present on the Object
    /// </summary>
    public static bool HasComponent<T>(this Component c)
    {
        return c.GetComponent<T>() != null;
    }
}
