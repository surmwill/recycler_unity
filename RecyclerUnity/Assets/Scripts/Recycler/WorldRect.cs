using UnityEngine;

/// <summary>
/// Represents a rectangle in world space
/// </summary>
public struct WorldRect
{
    /// <summary>
    /// The bottom left corner
    /// </summary>
    public Vector3 BotLeftCorner { get; }
    
    /// <summary>
    /// The top left corner
    /// </summary>
    public Vector3 TopLeftCorner { get; }
    
    /// <summary>
    /// The top right corner
    /// </summary>
    public Vector3 TopRightCorner { get; }
    
    /// <summary>
    /// The bot right corner
    /// </summary>
    public Vector3 BotRightCorner { get; }
    
    /// <summary>
    /// The normalized right vector
    /// </summary>
    public Vector3 Right => _right ?? (_right = (BotRightCorner - BotLeftCorner).normalized).Value;

    /// <summary>
    /// The normalized up vector
    /// </summary>
    public Vector3 Up => _up ?? (_up = (TopLeftCorner - BotLeftCorner).normalized).Value;
    
    /// <summary>
    /// The center
    /// </summary>
    public Vector3 Center { get; }

    /// <summary>
    /// The plane that the Rect lies on
    /// </summary>
    public Plane Plane { get; }

    /// <summary>
    /// The width of the rectangle
    /// </summary>
    public float Width => _width ?? (_width = (BotLeftCorner - BotRightCorner).magnitude).Value;

    /// <summary>
    /// The height of the rectangle
    /// </summary> 
    public float Height => _height ?? (_height = (TopLeftCorner - BotLeftCorner).magnitude).Value;
    
    private static readonly Vector3[] CachedGetWorldCorners = new Vector3[4];

    private float? _width;
    private float? _height;

    private Vector3? _right;
    private Vector3? _up;

    public WorldRect(RectTransform rect)
    {
        rect.GetWorldCorners(CachedGetWorldCorners);
        (BotLeftCorner, TopLeftCorner, TopRightCorner, BotRightCorner) = (CachedGetWorldCorners[0], CachedGetWorldCorners[1], CachedGetWorldCorners[2], CachedGetWorldCorners[3]);
        
        Center = BotLeftCorner + (TopLeftCorner - BotLeftCorner) * 0.5f + (TopRightCorner - TopLeftCorner) * 0.5f;
        Plane = new Plane(BotLeftCorner, TopLeftCorner, TopRightCorner);

        (_width, _height, _right, _up) = (null, null, null, null);
    }

    /// <summary>
    /// Returns true if the Rect contains the given world point
    /// </summary>
    public bool Contains(Vector3 worldPoint)
    {
        if (Plane.ClosestPointOnPlane(worldPoint) != worldPoint)
        {
            return false;
        }

        (Vector3 leftEdge, Vector3 topEdge) = (TopLeftCorner - BotLeftCorner, TopRightCorner - TopLeftCorner);
        (Vector3 botLeftToPoint, Vector3 topLeftToPoint) = (worldPoint - BotLeftCorner, worldPoint - TopLeftCorner);
        (float dotWithLeftEdge, float dotWithTopEdge) = (Vector3.Dot(leftEdge, botLeftToPoint), Vector3.Dot(topEdge, topLeftToPoint));
        
        return dotWithLeftEdge >= 0 && dotWithLeftEdge <= leftEdge.sqrMagnitude &&
               dotWithTopEdge >= 0 && dotWithTopEdge <= topEdge.sqrMagnitude;
    }

    /// <summary>
    /// String representation
    /// </summary>
    public override string ToString()
    {
        return $"Bot Left {BotLeftCorner.PrecisePrint()}\n" +
               $"Top Left {TopLeftCorner.PrecisePrint()}\n" +
               $"Top Right {TopRightCorner.PrecisePrint()}\n" +
               $"Bot Right {BotRightCorner.PrecisePrint()}";
    }
}