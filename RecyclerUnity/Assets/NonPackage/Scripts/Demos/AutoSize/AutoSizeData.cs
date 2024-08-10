using System;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Data to test an auto-sized recycler entry
    /// </summary>
    public class AutoSizeData : IRecyclerScrollRectData<string>
    {
        /// <summary>
        /// The key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The initial number of lines of text
        /// </summary>
        public int NumLines { get; }
        
        /// <summary>
        /// Appended lines of text
        /// </summary>
        public int NumAppendedLines { get; set; }

        public AutoSizeData(int numLines)
        {
            NumLines = numLines;
            Key = Guid.NewGuid().ToString();
        }
    }
}
