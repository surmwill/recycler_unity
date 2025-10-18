using System;
using UnityEngine;

namespace com.swill.recycler
{
    /// <summary>
    /// Endcap for testing a recycler with full screen entries and endcap.
    /// </summary>
    public class FullScreenEntriesEndcap : RecyclerScrollRectEndcap<EmptyRecyclerData, string>
    {
        private const float BufferPct = 0.2f;
        
        protected override void OnFetchedFromPool()
        {
            // Add a bit of a buffer just to be safe
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(Screen.height + Screen.height * BufferPct);
        }
    }
}
