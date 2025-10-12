using System;
using System.Collections.Generic;
using System.Linq;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Empty data to send to the recycler if we need to test simple things, like if entries are being created
    /// </summary>
    public class EmptyRecyclerData : IRecyclerScrollRectData<string>
    {
        public string Key { get; } = Guid.NewGuid().ToString();

        public static IEnumerable<EmptyRecyclerData> GenerateEmptyData(int count)
        {
            return Enumerable.Repeat<object>(null, count).Select(_ => new EmptyRecyclerData());
        }
    }
}
