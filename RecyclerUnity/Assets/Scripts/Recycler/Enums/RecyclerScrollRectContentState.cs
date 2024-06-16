using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The states that Recycler content (either the entries or an endcap) can be in
/// </summary>
public enum RecyclerScrollRectContentState
{
    InactiveInPool = -1,
    ActiveVisible = 0,
    ActiveInStartCache = 1,
    ActiveInEndCache = 2,
}
