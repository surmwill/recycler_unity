
namespace RecyclerScrollRect
{
    public static class RecyclerScrollRectContentStateExtensions
    {
        public static bool IsInCache(this RecyclerScrollRectContentState state)
        {
            return state == RecyclerScrollRectContentState.ActiveInStartCache || state == RecyclerScrollRectContentState.ActiveInEndCache;
        }
    }
}
