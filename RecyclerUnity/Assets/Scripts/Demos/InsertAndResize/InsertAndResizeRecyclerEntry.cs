using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A recycler entry to be fed null data
/// </summary>
public class InsertAndResizeRecyclerEntry : RecyclerScrollRectEntry<bool>
{
    private bool _shouldInsertAndResize;

    protected override void OnBindNewData(bool shouldInsertAndSize)
    {
        _shouldInsertAndResize = shouldInsertAndSize;
    }

    protected override void OnRebindExistingData()
    {
    }

    protected override void OnSentToRecycling()
    {
        
    }
}
