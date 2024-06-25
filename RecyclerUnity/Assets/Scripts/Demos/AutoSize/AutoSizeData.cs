using System;

/// <summary>
/// Data to test an auto-sized recycler entry
/// </summary>
public class AutoSizeData : IRecyclerScrollRectData<string>
{
    public string Key => Guid.NewGuid().ToString();
    
    public int NumLines { get; }

    public AutoSizeData(int numLines)
    {
        NumLines = numLines;
    }
}
