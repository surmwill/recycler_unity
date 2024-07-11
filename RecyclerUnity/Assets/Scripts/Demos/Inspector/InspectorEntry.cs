using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Entry to test inspector options
/// (ex: increasing/decreasing the pool size and having the corresponding amount of GameObjects)
/// </summary>
public class InspectorEntry : RecyclerScrollRectEntry<InspectorData, string>
{
    protected override void OnBindNewData(InspectorData entryData)
    {
    }

    protected override void OnRebindExistingData()
    {
    }

    protected override void OnSentToRecycling()
    {
    }
}
