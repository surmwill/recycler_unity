
namespace RecyclerScrollRect
{
    /// <summary>
    /// The states that Recycler entries or the endcap can be in.
    /// </summary>
    public enum RecyclerScrollRectContentState
    {
        InactiveInPool = 0,         // The object is inactive in its pool, waiting to become active
        ActiveVisible = 1,          // The object is active and visible on-screen
        ActiveInStartCache = 2,     // The object is active, waiting just offscreen to be scrolled to
        ActiveInEndCache = 3,       // The object is active, waiting just offscreen to be scrolled to
    }
}
