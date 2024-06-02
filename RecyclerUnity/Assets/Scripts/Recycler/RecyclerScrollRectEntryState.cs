using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The states a RecyclerScrollRectEntry can be in
/// </summary>
public enum RecyclerScrollRectEntryState
{
    // In recycling pool
    InPoolUnbound = -2,
    InPoolBound = -1,
    
    // Visible on screen
    Visible = 0,
    
    // Active GameObjects, but sitting offscreen waiting to be scrolled to
    InStartCache = 1,
    InEndCache = 2,
}
