using System;
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
        
        private RecyclerValidityChecker<EmptyRecyclerData, string> _validityChecker;

        private void Start()
        {
            _validityChecker = new RecyclerValidityChecker<EmptyRecyclerData, string>(_deleteRecyclerScrollRect);
            _validityChecker.Bind();
            
            _deleteRecyclerScrollRect.AppendEntries(EmptyRecyclerData.GenerateEmptyData(NumEntries));
        }

        private void OnDestroy()
        {
            _validityChecker.Unbind();
        }

        private void Update()
        {
            IReadOnlyDictionary<int, RecyclerScrollRectEntry<EmptyRecyclerData, string>> activeEntries = _deleteRecyclerScrollRect.ActiveEntries;

            if (Input.GetKeyDown(KeyCode.A) && Array.TrueForAll(IndicesToRemove, index => activeEntries.ContainsKey(index)))
            {
                foreach (int index in IndicesToRemove)
                {
                    ((DeleteRecyclerEntry) activeEntries[index]).Delete();
                }
            }
        }

        private void OnValidate()
        {
            if (_deleteRecyclerScrollRect == null)
            {
                _deleteRecyclerScrollRect = GetComponent<DeleteRecyclerScrollRect>();
            }
        }
    }
}
