using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingWindow<TEntryData>
{
    private readonly int _numCached;
    
    public bool IsDirty { get; private set; }

    private int _visibleStartIndex;
    private int _visibleEndIndex;
    
    public int VisibleStartIndex
    {
        get => _visibleStartIndex;
        private set
        {
            IsDirty = value != _visibleStartIndex;
            _visibleStartIndex = value;
        }
    }

    public int VisibleEndIndex
    {
        get => _visibleEndIndex;
        private set
        {
            IsDirty = value != _visibleEndIndex;
            _visibleEndIndex = value;
        }
    }

    public int CachedStartIndex => Mathf.Max(VisibleStartIndex - _numCached, 0);
    public int CachedEndIndex => Mathf.Max(VisibleEndIndex + _numCached);

    public void Insert(int index)
    {
        VisibleEndIndex += Contains(index) ? 1 : 0;
    }

    public bool IsVisible(int index)
    {
        return index >= VisibleStartIndex && index <= VisibleEndIndex;
    }

    public bool IsInStartCache(int index)
    {
        return index >= CachedStartIndex && index < VisibleStartIndex;
    }

    public bool IsInEndCache(int index)
    {
        return index > CachedEndIndex && index <= CachedEndIndex + _numCached;
    }

    public bool Contains(int index)
    {
        return IsVisible(index) || IsInStartCache(index) || IsInEndCache(index);
    }

    public SlidingWindow(int numCached)
    {
        
    }
}
