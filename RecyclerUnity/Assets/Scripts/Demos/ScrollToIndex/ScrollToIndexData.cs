using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data used to demo scrolling to an index in a recycler
/// </summary>
public class ScrollToIndexData
{
   public bool ShouldResize { get; private set; }

   public ScrollToIndexData(bool shouldResize)
   {
      ShouldResize = shouldResize;
   }
}