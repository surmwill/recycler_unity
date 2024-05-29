using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data for recycler entries used in our insert and resize demo
/// </summary>
public class InsertAndResizeData
{
    public bool ShouldGrow { get; }
    
    public bool DidGrow { get; set; }

    public InsertAndResizeData(bool shouldGrow)
    {
        ShouldGrow = shouldGrow;
    }
}
