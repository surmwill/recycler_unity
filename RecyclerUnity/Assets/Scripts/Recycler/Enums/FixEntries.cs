using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When we insert, remove, or modify an entry the size of our list changes.
/// This defines how the entries should get pushed around on a size change (or rather, which entries should not)
/// </summary>
public enum FixEntries
{
    Below = 0,  // All entries below where the existing/new entry is modified will be unmoved 
    Above = 1,  // All entries above where the existing/new entry is modified will be unmoved
    Mid = 2,    // The center where the existing/new entry is 
}
