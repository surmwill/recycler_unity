using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Swill.Recycler
{
    /// <summary>
    /// Entry to test inspector options.
    /// (Swapping entry prefab and ensuring pool is regenerated.)
    /// </summary>
    public class InspectorEntryOne : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        protected override void OnBind(EmptyRecyclerData entryData)
        {
        }
    }
}
