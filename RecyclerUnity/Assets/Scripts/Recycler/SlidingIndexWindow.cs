using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlidingIndexWindow : ISlidingIndexWindow
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

    /// <summary>
    /// The window needs a start and an end index. Prior to any data (i.e. an empty recycler) these do not exist and so the window does not exist.
    /// </summary>
    public bool Exists => VisibleStartIndex.HasValue && VisibleEndIndex.HasValue;

    public int CachedStartIndex => Mathf.Max(VisibleStartIndex.GetValueOrDefault() - _numCached, 0);
    public int CachedEndIndex => Mathf.Max(VisibleEndIndex.GetValueOrDefault() + _numCached, 0);

    public void InsertRange(int index, int num)
    {
        IsDirty = true;

        if (index >= VisibleStartIndex && index <= VisibleEndIndex)
        {
            VisibleEndIndex += num;
        }
        
        if (index == VisibleStartIndex)
        {
            VisibleStartIndex += num;
        }
    }

    public void Remove(int index)
    {
        IsDirty = true;

        if (index >= VisibleStartIndex && index <= VisibleEndIndex)
        {
            VisibleEndIndex--;
        }
        
        if (index == VisibleStartIndex)
        {
            VisibleStartIndex++;
        }
    }
    
    public void Reset()
    {
        (VisibleStartIndex, VisibleEndIndex) = (null, null);
        IsDirty = false;
    }

    public bool IsVisible(int index)
    {
        return Exists && index >= VisibleStartIndex && index <= VisibleEndIndex;
    }

    public bool IsInStartCache(int index)
    {
        return Exists && index >= CachedStartIndex && index < VisibleStartIndex;
    }

    public bool IsInEndCache(int index)
    {
        return Exists && index > VisibleEndIndex && index <= CachedEndIndex;
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
