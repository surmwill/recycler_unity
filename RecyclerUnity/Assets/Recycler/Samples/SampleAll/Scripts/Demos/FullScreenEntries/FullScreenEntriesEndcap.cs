using System;
using UnityEngine;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Endcap for testing a recycler with full screen entries and endcap.
    /// </summary>
    public class FullScreenEntriesEndcap : RecyclerScrollRectEndcap<string, EmptyRecyclerData>
    {
        private const float Buffer = 1.2f;
        
        protected override void OnFetchedFromPool()
        {
            // Add a bit of a buffer just to be safe
            RectTransform.sizeDelta = Recycler.Orientation.IsVertical() ? RectTransform.sizeDelta.WithY(Screen.height * Buffer) : RectTransform.sizeDelta.WithX(Screen.width * Buffer);
        }
    }
}
