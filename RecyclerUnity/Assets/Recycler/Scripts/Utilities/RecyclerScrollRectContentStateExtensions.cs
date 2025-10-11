
namespace RecyclerScrollRect
{
    public static class RecyclerScrollRectContentStateExtensions
    {
        public static bool IsInCache(this RecyclerScrollRectContentState state)
        {
            return state == RecyclerScrollRectContentState.InStartCache || state == RecyclerScrollRectContentState.InEndCache;
        }
    }
}
