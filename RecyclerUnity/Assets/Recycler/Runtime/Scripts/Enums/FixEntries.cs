
namespace Swill.Recycler
{
    /// <summary>
    /// If we're updating the size of a visible entry, then we'll either be pushing other entries or creating extra space for other entries to occupy.
    /// This defines how and what entries will get moved. If we're not updating an entry in the visible window, this is ignored,
    /// and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
    /// </summary>
    public enum FixEntries
    {
        VerticalBelow = 0,      // All entries below the one modified will stay unmoved. 
        VerticalAbove = 1,      // All entries above the one modified will stay unmoved. 
        
        HorizontalLeft = 2,       // All entries to the left of the one modified will stay unmoved.
        HorizontalRight = 3,      // All entries to the right of the one modified will stay unmoved.
        
        Middle = 4,        // All entries above and below the one modified, or to the left and right of the one modified, will be moved equally.
    }
}
