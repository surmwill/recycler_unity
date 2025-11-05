
namespace Swill.Recycler
{
    /// <summary>
    /// Defines the position within an entry to center on when we scroll to it.
    /// </summary>
    public enum ScrollToAlignment
    {
        VerticalEntryBottom = 0,    // Center on the bottom edge of the entry.
        VerticalEntryTop = 1,       // Center on the top edge of the entry.
        
        HorizontalEntryLeft = 2,      // Center on the left edge of the entry.
        HorizontalEntryRight = 3,     // Center on the right edge of the entry.
        
        EntryMiddle = 4,    // Center on the middle of the entry.
    }
}
