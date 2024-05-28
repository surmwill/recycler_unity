using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The type of states a recycler entry can be in
/// </summary>
public enum RecyclerEntryState
{
    // Active under ScrollRect
    Visible = 0,
    Cached = 1,
    
    // Inactive under Pool
    PooledBound = 2,
    PooledUnbound = 3,
}
