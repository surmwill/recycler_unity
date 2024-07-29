using System;

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

        public PrettyInsertDeleteData(bool animateIn)
        {
            Key = Guid.NewGuid().ToString();
            AnimateIn = animateIn;
        }
    }
}
