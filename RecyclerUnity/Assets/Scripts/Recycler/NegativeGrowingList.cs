using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Allows us to prepend to a list with the prepended entries having negative indexes; importantly not causing any shuffling
/// </summary>
public class NegativeGrowingList<T> : IEnumerable<T>
{
    private readonly List<T> _forward = new();
    private readonly List<T> _backward = new();
    
    /// <summary>
    /// The total number elements with indices < 0
    /// </summary>
    public int BackwardCount => _backward.Count;

    /// <summary>
    /// The total elements with indices >= 0
    /// </summary>
    public int ForwardCount => _forward.Count;

    /// <summary>
    /// The total number of elements
    /// </summary>
    public int Count => _forward.Count + _backward.Count;

    /// <summary>
    /// Adds elements to the front (positive end) of the list
    /// </summary>
    public void Append(IEnumerable<T> data)
    {
        _forward.AddRange(data);
    }

    /// <summary>
    /// Adds elements to the back (negative end) of the list
    /// </summary>
    public void Prepend(IEnumerable<T> data)
    {
        _backward.AddRange(data);
    }

    /// <summary>
    /// Inserts an element at an index
    /// </summary>
    public void Insert(int index, T data)
    {
        if (index >= 0)
        {
            _forward.Insert(index, data);
            return;
        }
        _backward.Insert(ToBackwardIndex(index), data);
    }

    /// <summary>
    /// Removes an element at an index
    /// </summary>
    public void RemoveAt(int index)
    {
        if (index >= 0)
        {
            _forward.RemoveAt(index);
            return;
        }
        _backward.RemoveAt(ToBackwardIndex(index));
    }

    /// <summary>
    /// Clears the list of all entries
    /// </summary>
    public void Clear()
    {
        _forward.Clear();
        _backward.Clear();
    }

    /// <summary>
    /// Index into the list
    /// </summary>
    public T this[int index]
    {
        get => index >= 0 ? _forward[index] : _backward[ToBackwardIndex(index)];
        set 
        {
            if (index >= 0)
            {
                _forward[index] = value;
                return;
            }
            _backward[ToBackwardIndex(index)] = value;
        }
    }
    
    /// <summary>
    /// Maps negative indices to their corresponding index in the backwards (prepended) List of elements.
    /// For example, an index of -1 corresponds behind-the-scenes to index 0 in the backwards list.
    /// </summary>
    private static int ToBackwardIndex(int index)
    {
        Assert.IsTrue(index < 0);
        return Mathf.Abs(index) - 1;
    }
    
    #region ENUMERATION
    
    public IEnumerator<T> GetEnumerator()
    {
        return _backward.AsEnumerable().Reverse().Concat(_forward).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    #endregion
}
