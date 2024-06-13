using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Empty data to send to the recycler if we just need to test simple things, like if entries are being created
/// </summary>
public class EmptyRecyclerData : IRecyclerScrollRectData<string>
{
    public string Key { get; }
    
    public EmptyRecyclerData()
    {
        Key = Guid.NewGuid().ToString();
    }

    public static IEnumerable<EmptyRecyclerData> GenerateEmptyData(int count)
    {
        return Enumerable.Repeat<object>(null, count).Select(_ => new EmptyRecyclerData());
    }
}
