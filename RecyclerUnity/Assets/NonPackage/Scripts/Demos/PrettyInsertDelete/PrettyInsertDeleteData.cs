using System;
using System.Collections.Generic;
using System.Linq;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Data for animating an entry in on insertion/deletion
    /// </summary>
    public class PrettyInsertDeleteData : IRecyclerScrollRectData<string>
    {
        /// <summary>
        /// The key
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Whether we should animate the entry in upon insertion
        /// </summary>
        public bool AnimateIn { get; set; }
        
        /// <summary>
        /// How we should fix the other entries if we are animating in the entry
        /// </summary>
        public FixEntries AnimateInFixEntries { get; }

        public PrettyInsertDeleteData(bool animateIn, FixEntries animateInFixEntries)
        {
            Key = Guid.NewGuid().ToString();
            AnimateIn = animateIn;
            AnimateInFixEntries = animateInFixEntries;
        }

        /// <summary>
        /// Generates a number of pieces of data
        /// </summary>
        public static IEnumerable<PrettyInsertDeleteData> GenerateData(int num, bool animateIn, FixEntries animateInFixEntries = FixEntries.Mid)
        {
            return Enumerable.Repeat<object>(null, num).Select(_ => new PrettyInsertDeleteData(animateIn, animateInFixEntries));
        }
    }
}
