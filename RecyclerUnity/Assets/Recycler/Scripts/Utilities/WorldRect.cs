using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Represents the 4 corners of a RectTransform in world space.
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
        /// The center
        /// </summary>
        public Vector3 Center { get; }

        /// <summary>
        /// The width of the rectangle
        /// </summary>
        public float Width => (BotLeftCorner - BotRightCorner).magnitude;

        /// <summary>
        /// The height of the rectangle
        /// </summary> 
        public float Height => (TopLeftCorner - BotLeftCorner).magnitude;

        private static readonly Vector3[] CachedGetWorldCorners = new Vector3[4];

        public WorldRect(RectTransform rect)
        {
            rect.GetWorldCorners(CachedGetWorldCorners);
            (BotLeftCorner, TopLeftCorner, TopRightCorner, BotRightCorner) = (CachedGetWorldCorners[0],
                CachedGetWorldCorners[1], CachedGetWorldCorners[2], CachedGetWorldCorners[3]);

            Center = BotLeftCorner + (TopLeftCorner - BotLeftCorner) * 0.5f + (TopRightCorner - TopLeftCorner) * 0.5f;
        }

        /// <summary>
        /// String representation of the WorldRect.
        /// </summary>
        /// <returns> A string representation of the WorldRect. </returns>
        public override string ToString()
        {
            return $"Bot Left {BotLeftCorner.PrecisePrint()}\n" +
                   $"Top Left {TopLeftCorner.PrecisePrint()}\n" +
                   $"Top Right {TopRightCorner.PrecisePrint()}\n" +
                   $"Bot Right {BotRightCorner.PrecisePrint()}";
        }
    }
}