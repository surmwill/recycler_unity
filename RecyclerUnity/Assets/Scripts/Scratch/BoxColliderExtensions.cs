using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Extensions for BoxColliders
/// </summary>
public static class BoxColliderExtensions
{
    /// <summary>
    /// Returns true if the BoxCollider contains the point
    /// </summary>
    public static bool ContainsPoint(this BoxCollider boxCollider, Vector3 point)
    {
        return boxCollider.ClosestPoint(point) == point;
    }
}
