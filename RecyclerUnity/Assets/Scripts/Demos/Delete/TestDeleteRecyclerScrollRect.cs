using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
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
            _deleteRecyclerScrollRect.AppendEntries(EmptyRecyclerData.GenerateEmptyData(NumEntries));
        }

        private void Update()
        {
            IReadOnlyDictionary<int, RecyclerScrollRectEntry<EmptyRecyclerData, string>> activeEntries =
                _deleteRecyclerScrollRect.ActiveEntries;

            if (Input.GetKeyDown(KeyCode.A) &&
                Array.TrueForAll(IndicesToRemove, index => activeEntries.ContainsKey(index)))
            {
                foreach (int index in IndicesToRemove)
                {
                    ((DeleteRecyclerEntry) activeEntries[index]).Delete();
                }
            }
        }
    }
}
