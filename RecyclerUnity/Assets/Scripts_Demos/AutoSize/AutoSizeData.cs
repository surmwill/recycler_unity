using System;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Data to test an auto-sized recycler entry
    /// </summary>
    public class AutoSizeData : IRecyclerScrollRectData<string>
    {
        public string Key { get; }

        public int NumLines { get; }

        public AutoSizeData(int numLines)
        {
            NumLines = numLines;
            Key = Guid.NewGuid().ToString();
        }
    }
}
