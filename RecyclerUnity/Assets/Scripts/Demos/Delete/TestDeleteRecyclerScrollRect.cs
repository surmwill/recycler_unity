using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Demos deleting a couple entries in the recycler
/// </summary>
public class TestDeleteRecyclerScrollRect : MonoBehaviour
{
    [SerializeField]
    private DeleteRecyclerScrollRect _deleteRecyclerScrollRect = null;

    private const int NumEntries = 50;

    private const int StartIndex = 15;
    private static readonly int[] IndicesToRemove = { StartIndex, StartIndex + 2, StartIndex + 3 };

    private void Start()
    {
        _deleteRecyclerScrollRect.AppendEntries(new object[NumEntries]);
    }

    private void Update()
    {
        IReadOnlyDictionary<int, RecyclerScrollRectEntry<object>> activeEntries = _deleteRecyclerScrollRect.ActiveEntries;
        
        if (Input.GetKeyDown(KeyCode.A) && Array.TrueForAll(IndicesToRemove, index => activeEntries.ContainsKey(index)))
        {
            Debug.Log("YES");
            foreach (int index in IndicesToRemove)
            {
                ((DeleteEntry) activeEntries[index]).Delete();
            }
        }
    }
}
