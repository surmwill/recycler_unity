using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Endcap to test inspector options
    /// (ex: switching endcaps and ensuring the old one gets replaced with the new one)
    /// </summary>
    public class InspectorEndcapOne : RecyclerScrollRectEndcap<EmptyRecyclerData, string>
    {
        public override void OnFetchedFromRecycling()
        {
        }

        public override void OnSentToRecycling()
        {
        }
    }
}
