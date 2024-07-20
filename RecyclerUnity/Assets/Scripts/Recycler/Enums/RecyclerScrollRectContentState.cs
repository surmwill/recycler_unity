using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// The states that Recycler entries (or the endcap) can be in
    /// </summary>
    public enum RecyclerScrollRectContentState
    {
        InactiveInPool = -1,
        ActiveVisible = 0,
        ActiveInStartCache = 1,
        ActiveInEndCache = 2,
    }
}
