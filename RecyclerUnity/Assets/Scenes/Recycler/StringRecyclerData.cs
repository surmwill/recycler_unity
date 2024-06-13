using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sample data to send the recycler
/// </summary>
public class StringRecyclerData : IRecyclerScrollRectData<string>
{
    public string Key { get; }
    
    public string Data { get; }

    public StringRecyclerData(string data)
    {
        Key = data;
        Data = data;
    }
}
