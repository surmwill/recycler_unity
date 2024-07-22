using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Entry to test inspector options
    /// (ex: increasing/decreasing the pool size and having the corresponding amount of GameObjects)
    /// </summary>
    public class InspectorEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        protected override void OnBindNewData(EmptyRecyclerData entryData, RecyclerScrollRectContentState onBindState)
        {
        }
    }
}
