using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlidingIndexWindow
{
    private readonly int _numCached;
    
    public bool IsDirty { get; set; }

    private int? _visibleStartIndex;
    private int? _visibleEndIndex;
    
    public int? VisibleStartIndex
    {
        get => _visibleStartIndex;
        set
        {
            IsDirty = value != _visibleStartIndex;
            _visibleStartIndex = value;
        }
    }

    public int? VisibleEndIndex
    {
        get => _visibleEndIndex;
        set
        {
            IsDirty = value != _visibleEndIndex;
            _visibleEndIndex = value;
        }
    }

    public int CachedStartIndex => Mathf.Max(VisibleStartIndex.GetValueOrDefault() - _numCached, 0);
    public int CachedEndIndex => Mathf.Max(VisibleEndIndex.GetValueOrDefault() + _numCached, 0);

    public void Insert(int index)
    {
        if (Contains(index))
        {
            VisibleEndIndex++;
        }
    }

    public void Remove(int index)
    {
        if (IsInStartCache(index))
        {
            VisibleStartIndex--;
            VisibleEndIndex--;
        }
        else if (IsVisible(index))
        {
            VisibleEndIndex--;
        }
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
        return index > VisibleEndIndex && index <= CachedEndIndex + _numCached;
    }

    public bool Contains(int index)
    {
        return IsVisible(index) || IsInStartCache(index) || IsInEndCache(index);
    }

    public string PrintRange()
    {
        return $"Visible Index Range: [{VisibleStartIndex},{VisibleEndIndex}]\n" +
               $"Start Cache Range: [{CachedStartIndex}, {VisibleStartIndex})\n" +
               $"End Cache Range: ({VisibleEndIndex}, {CachedEndIndex}]";
    }

    public SlidingIndexWindow(int numCached)
    {
        _numCached = numCached;
    }
    
}
