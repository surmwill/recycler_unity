using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Identify where the insertion, deletion, or re-size position is.
/// All entries either below it or above it will maintain their positions as the new entry is added, removed, or modified.
/// The opposite sided entries will get pushed in proportion to the height increase or decrease.
/// </summary>
public enum FixEntries
{
    Below = 0,
    Above = 1,
}
