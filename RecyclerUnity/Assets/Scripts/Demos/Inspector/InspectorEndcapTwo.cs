using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Endcap to test inspector options
/// (ex: switching endcaps and ensuring the old one gets replaced with the new one)
/// </summary>
public class InspectorEndcapTwo : RecyclerScrollRectEndcap<EmptyRecyclerData, string>
{
    public override void OnFetchedFromRecycling()
    {
    }

    public override void OnSentToRecycling()
    {
    }
}
