using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Endcap for testing a recycler with full screen entries and endcap.
    /// </summary>
    public class FullScreenEntriesEndcap : RecyclerScrollRectEndcap<EmptyRecyclerData, string>
    {
        private const float BufferPct = 0.1f;
        
        public override void OnFetchedFromPool()
        {
            // Add a bit of a buffer just to be safe
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(Screen.height + Screen.height * BufferPct);
        }
    }
}
