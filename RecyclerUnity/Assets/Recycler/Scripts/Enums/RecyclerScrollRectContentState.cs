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
        InactiveInPool = 0,
        ActiveVisible = 1,
        ActiveInStartCache = 2,
        ActiveInEndCache = 3,
    }
}
