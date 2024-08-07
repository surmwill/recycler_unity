using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Example of recycler data for demoing purposes
    /// </summary>
    public class DemoRecyclerData : IRecyclerScrollRectData<string>
    {
        /// <summary>
        /// IRecyclerScrollRectData implementation
        /// </summary>
        public string Key => Word;

        public string Word { get; private set; }

        public Color BackgroundColor { get; private set; }

        public DemoRecyclerData(string word, Color backgroundColor)
        {
            Word = word;
            BackgroundColor = backgroundColor;
        }
    }
}
