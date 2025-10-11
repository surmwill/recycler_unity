
namespace RecyclerScrollRect
{
    /// <summary>
    /// The states that Recycler entries or the endcap can be in.
    /// </summary>
    public enum RecyclerScrollRectContentState
    {
        Visible = 0,          // The object is active and visible on-screen
        InStartCache = 1,     // The object is active, waiting just offscreen to be scrolled to
        InEndCache = 2,       // The object is active, waiting just offscreen to be scrolled to
    }
}
