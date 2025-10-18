
namespace RecyclerScrollRect
{
    /// <summary>
    /// Interface for the data bound to entries in the RecyclerScrollRect.
    /// Each piece of data must provide a unique key relative to the full list of data.
    /// </summary>
    public interface IRecyclerScrollRectData<out TEntryDataKey>
    {
        /// <summary>
        /// A unique key identifying the piece of data.
        /// </summary>
        TEntryDataKey Key { get; }
    }
}
