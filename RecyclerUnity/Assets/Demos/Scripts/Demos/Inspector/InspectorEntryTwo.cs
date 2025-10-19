using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Entry to test inspector options.
    /// (Swapping entry prefab and ensuring pool is regenerated.)
    /// </summary>
    public class InspectorEntryTwo : RecyclerScrollRectEntry<string, EmptyRecyclerData>
    {
        protected override void OnBind(EmptyRecyclerData entryData)
        {
        }
    }
}
