
namespace RecyclerScrollRect
{
    /// <summary>
    /// When we insert or remove an entry or modify its size, and it's onscreen, other entries will need to get pushed around to accomodate its size change.
    /// This defines how the entries should get pushed around on a size change.
    /// </summary>
    public enum FixEntries
    {
        Below = 0, // All entries below the modified entry will stay unmoved 
        Above = 1, // All entries above the modified entry will stay unmoved 
        Mid = 2, // The modified entry will push the entries above and below it equally
    }
}
