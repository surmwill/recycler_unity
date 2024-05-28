using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface to be able to see the indices currently bound, but without the ability to modify the window
/// </summary>
public interface ISlidingIndexWindow
{
    public bool Exists { get; }
    
    public int? VisibleStartIndex { get; }

    public int? VisibleEndIndex { get; }

    public int CachedStartIndex { get; }
    
    public int CachedEndIndex { get; }
    
    public bool IsVisible(int index);

    public bool IsInStartCache(int index);

    public bool IsInEndCache(int index);

    public bool Contains(int index);

    public string PrintRange();
}
