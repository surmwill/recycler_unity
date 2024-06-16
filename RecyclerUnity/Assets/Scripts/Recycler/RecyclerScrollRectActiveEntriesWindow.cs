using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Represents a sliding range of entries currently active in the Recycler: either visible on screen, or just offscreen in the cache, ready to become visible 
/// </summary>
public class RecyclerScrollRectActiveEntriesWindow
{
    /// <summary>
    /// Returns true if the underlying has size > 0. The window can only exist if the underlying data exists
    /// </summary>
    public bool Exists => WindowSize > 0;
    
    /// <summary>
    /// The starting index of the range of visible entries 
    /// </summary>
    public int VisibleStartIndex => VisibleIndices?.Start ?? -1;

    /// <summary>
    /// The ending index of the range of visible entries
    /// </summary>
    public int VisibleEndIndex => VisibleIndices?.End ?? -1;

    /// <summary>
    /// The starting index of the range of entries contained in the start cache. The entry before the first visible entry is when the range stops.
    /// If no entries are in the start cache then -1 is returned.
    /// </summary>
    public int CachedStartIndex => !VisibleIndices.HasValue || VisibleIndices.Value.Start == 0 ? 
        -1 : 
        Mathf.Max(VisibleIndices.Value.Start - _numCached, 0);
    
    /// <summary>
    /// The ending index of the range of entries contained in the end cache. The entry after the last visible entry is when the range stops.
    /// If no entries are in the end cache then -1 is returned.
    /// </summary>
    public int CachedEndIndex => !Exists || (VisibleIndices.HasValue && VisibleIndices.Value.End == WindowSize - 1) ? 
        -1 : 
        Mathf.Min(VisibleIndices.HasValue ? VisibleIndices.Value.End + _numCached : _numCached - 1, WindowSize - 1);
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsDirty { get; private set; }

    public (int Start, int End)? VisibleIndices
    {
        get => _visibleIndicesBacking;
        set
        {
            (int Start, int End)? newVisibleIndices = value;
            IsDirty = IsDirty || 
                      _visibleIndicesBacking.HasValue != newVisibleIndices.HasValue || 
                      _visibleIndicesBacking?.Start != newVisibleIndices?.Start || 
                      _visibleIndicesBacking?.End != newVisibleIndices?.End;
            
            #if UNITY_EDITOR
            if (newVisibleIndices.HasValue && newVisibleIndices.Value.Start > newVisibleIndices.Value.End)
            {
                Debug.LogError($"The visible start index \"{newVisibleIndices.Value.Start}\" should not be greater than the end index \"{newVisibleIndices.Value.End}\"");
            }
            #endif

            _visibleIndicesBacking = newVisibleIndices;
        }
    }
    
    private int WindowSize
    {
        get => _windowSizeBacking;
        set
        {
            IsDirty = IsDirty || value != _windowSizeBacking;
            _windowSizeBacking = value;
        }
    }

    private readonly int _numCached;
    
    private (int Start, int End)? _visibleIndicesBacking;

    private int _windowSizeBacking;

    public void InsertRange(int index, int num)
    {
        if (index > WindowSize)
        {
            throw new ArgumentException($"index must \"{index}\" be non-negative and <= the window size \"{WindowSize}\"");
        }
        
        // Increase the size of the window
        WindowSize += num;
        
        if (!VisibleIndices.HasValue)
        {
            return;
        }
        
        // Shift the current window to accomodate the new entries. We assume the new entries are not visible until they tell us otherwise 
        (int Start, int End) shiftedVisibleIndices = VisibleIndices.Value;
        
        if (index <= VisibleEndIndex)
        {
            shiftedVisibleIndices.End += num;
        }
        
        if (index <= VisibleStartIndex)
        {
            shiftedVisibleIndices.Start += num;
        }

        VisibleIndices = shiftedVisibleIndices;
    }

    public void Remove(int index)
    {
        if (index < 0 || index >= WindowSize)
        {
            throw new ArgumentException($"index must \"{index}\" be non-negative and < the window size \"{WindowSize}\"");
        }
        
        // Decrease the size of the window
        WindowSize--;

        if (!VisibleIndices.HasValue)
        {
            return;
        }
        
        // If we've deleted everything then nothing is visible
        if (WindowSize == 0)
        {
            VisibleIndices = null;
            return;
        }

        // Shift the current window to accomodate the removed entries. We assume no new entries become visible until they tell us otherwise 
        (int Start, int End) shiftedVisibleIndices = VisibleIndices.Value;

        if (index <= VisibleEndIndex)
        {
            shiftedVisibleIndices.End--;
        }
        
        if (index < VisibleStartIndex)
        {
            shiftedVisibleIndices.Start--;
        }
        else if (index == VisibleStartIndex)
        {
            shiftedVisibleIndices.Start = Mathf.Min(shiftedVisibleIndices.End, shiftedVisibleIndices.Start + 1); 
        }

        VisibleIndices = shiftedVisibleIndices;
    }
    
    /// <summary>
    /// Resets the window to as if all entries were non-visible
    /// </summary>
    public void Reset()
    {
        VisibleIndices = null;
    }

    /// <summary>
    /// Returns true if the given index is visible
    /// </summary>
    public bool IsVisible(int index)
    {
        return VisibleIndices.HasValue && index >= VisibleStartIndex && index <= VisibleEndIndex;
    }

    /// <summary>
    /// Returns true if the given index is in the start cache (just outside the visible entries, ready to become visible) 
    /// </summary>
    public bool IsInStartCache(int index)
    {
        return VisibleIndices.HasValue && index >= CachedStartIndex && index < VisibleStartIndex;
    }

    /// <summary>
    /// Returns true if the given index is in the end cache (just outside the visible entries, ready to become visible)
    /// </summary>
    public bool IsInEndCache(int index)
    {
        return VisibleIndices.HasValue && index > VisibleEndIndex && index <= CachedEndIndex;
    }

    /// <summary>
    /// Returns true if the index is part of the active entries on screen: either visible on screen, or just offscreen in the cache, ready to become visible 
    /// </summary>
    public bool Contains(int index)
    {
        return IsVisible(index) || IsInStartCache(index) || IsInEndCache(index);
    }

    /// <summary>
    /// Relays that we have checked the current range of active entries, done any work we need to,
    /// and are now waiting on the window to become dirty again (the range of active entries changes)
    /// </summary>
    public void SetNonDirty()
    {
        IsDirty = false;
    }

    /// <summary>
    /// Returns information about the current window of active entries
    /// </summary>
    public string PrintRanges()
    {
        return $"Visible Index Range: [{VisibleStartIndex},{VisibleEndIndex}]\n" +
               $"Start Cache Range: [{CachedStartIndex}, {VisibleStartIndex})\n" +
               $"End Cache Range: ({VisibleEndIndex}, {CachedEndIndex}]";
    }

    public RecyclerScrollRectActiveEntriesWindow(int numCached)
    {
        _numCached = numCached;
    }
}
