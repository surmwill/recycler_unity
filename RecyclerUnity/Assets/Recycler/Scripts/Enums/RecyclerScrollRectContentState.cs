
namespace RecyclerScrollRect
{
    /// <summary>
    /// The states that Recycler entries or the endcap can be in.
    /// </summary>
    public enum RecyclerScrollRectContentState
    {
        Pooled = 0,           // The object is inactive and waiting in the recycling pool
        Visible = 1,          // The object is active and visible on-screen
        InStartCache = 2,     // The object is active, waiting just offscreen to be scrolled to
        InEndCache = 3,       // The object is active, waiting just offscreen to be scrolled to
    }
}
