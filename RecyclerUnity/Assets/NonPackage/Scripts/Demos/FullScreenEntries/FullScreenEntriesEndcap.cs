using System;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Endcap for testing a recycler with full screen entries and endcap.
    ///
    /// Note that a fullscreen endcap currently does not work. We base what is in the start and end cache off what entry is
    /// currently visible. If no entry is visible, then there shouldn't be anything to scroll to on or off-screen - that on
    /// or off-screen content should be what's visible. Hence we clear the caches when no entry is visible.
    ///
    /// However with a full-screen endcap this differs. The endcap we can push all the visible entries off screen, but these
    /// entries should still be alive in the start cache.
    /// 
    /// Remedying this requires treating the caches as disjoint from what's visible. This introduces a multitude of code changes
    /// and new test cases for a very specific situation and is currently not worth the time.
    /// </summary>
    public class FullScreenEntriesEndcap : RecyclerScrollRectEndcap<EmptyRecyclerData, string>
    {
        private const float BufferPct = 0.1f;
        
        public override void OnFetchedFromPool()
        {
            throw new Exception("A fullscreen endcap is currently not supported. Please read class comment for more.");
            
            // Add a bit of a buffer just to be safe
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(Screen.height + Screen.height * BufferPct);
        }
    }
}
