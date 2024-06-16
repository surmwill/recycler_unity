using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Our recycler is ultimately a list of GameObjects.
/// This defines where the starting entry (bound with index 0) falls in that list of transforms.
/// Top = lower sibling index - higher up in the scene hierarchy
/// </summary>
public enum RecyclerTransformPosition
{
    Top = 0,
    Bot = 1,
}
