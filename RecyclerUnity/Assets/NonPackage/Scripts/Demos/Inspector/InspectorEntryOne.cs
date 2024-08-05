using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Entry to test inspector options.
    /// (Swapping entry prefab and ensuring pool is regenerated.)
    /// </summary>
    public class InspectorEntryOne : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        protected override void OnBindNewData(EmptyRecyclerData entryData)
        {
        }
    }
}
