using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the position within an entry to center on when we scroll to it.
/// For example, we can scroll to an entry's middle or its top edge.
/// </summary>
public enum ScrollToAlignment
{
    EntryMiddle = 0,    // Center on the middle of the entry
    EntryBottom = 1,    // Center on the bottom edge of the entry
    EntryTop = 2,       // Center on the top edge of the entry
}
