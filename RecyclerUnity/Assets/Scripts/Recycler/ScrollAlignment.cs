using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the position within an entry to scroll to (i.e. this position will end up in the viewport center).
/// If we have a large entry for example, we might want to scroll to the bottom of it instead of the middle.
/// </summary>
public enum ScrollAlignment
{
    Middle = 0,
    Bottom = 1,
    Top = 2,
}
