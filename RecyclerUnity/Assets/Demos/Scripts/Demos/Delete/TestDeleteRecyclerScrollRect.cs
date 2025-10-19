using System.Linq;
using UnityEngine;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Demos deleting a entries in the recycler.
    /// </summary>
    public class TestDeleteRecyclerScrollRect : TestRecycler<EmptyRecyclerData, string>
    {
        [SerializeField]
        private DeleteRecyclerScrollRect _deleteRecycler = null;

        private const int InitNumEntries = 50;

        private const int DeleteAtIndex = 15;
        private const int NumEntriesToDelete = 3;

        protected override RecyclerScrollRect<EmptyRecyclerData, string> ValidateRecycler => _deleteRecycler;

        protected override string DemoTitle => "Deletion Demo";

        protected override string DemoDescription => $"Tests deletion of entries.";

        protected override string[] DemoButtonDescriptions => new[]
        {
            $"0 (key 'A'): Starting starting at {DeleteAtIndex}, shrinks and deletes {NumEntriesToDelete}.",
            $"1 (key 'D'): Batch deletes the last {NumEntriesToDelete} entries instantly.",
            $"2 (key 'C'): Deletes the entire range of active entries instantly.",
            $"3 (key 'R'): Shrinks and deletes an entry from a random active index."
        };
        
        private IRecyclerScrollRectActiveEntriesWindow<EmptyRecyclerData, string> _activeEntriesWindow;

        protected override void Start()
        {
            base.Start();
            _deleteRecycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitNumEntries));
            _activeEntriesWindow = _deleteRecycler.ActiveEntriesWindow;
        }

        private void Update()
        {
            (int Start, int End)? activeEntriesRange = _activeEntriesWindow.ActiveEntriesRange;
            
            // Shrink and delete delete
            if (Input.GetKeyDown(KeyCode.A) || DemoToolbar.GetButtonDown(0))
            {
                string[] deleteKeys = Enumerable.Range(DeleteAtIndex, NumEntriesToDelete)
                    .Where(i => i < _deleteRecycler.DataForEntries.Count)
                    .Select(i => _deleteRecycler.DataForEntries[i].Key).ToArray();
                
                foreach (string key in deleteKeys)
                {
                    if (_deleteRecycler.TryGetActiveEntryWithKey(key, out DeleteRecyclerEntry entry))
                    {
                        entry.ShrinkAndDelete();
                    }
                    else
                    {
                        _deleteRecycler.RemoveAtKey(key);
                    }
                }
            }
            // Immediate batch delete from the end.
            else if (Input.GetKeyDown(KeyCode.D) || DemoToolbar.GetButtonDown(1))
            { 
                _deleteRecycler.RemoveRangeAtIndex(
                    Mathf.Max(_deleteRecycler.DataForEntries.Count - NumEntriesToDelete, 0), 
                    Mathf.Min(NumEntriesToDelete, _deleteRecycler.DataForEntries.Count));
            }
            // Delete the entire range of active entries.
            else if ((Input.GetKeyDown(KeyCode.C) || DemoToolbar.GetButtonDown(2)) && activeEntriesRange.HasValue)
            {
                (int Start, int End) = activeEntriesRange.Value;
                _deleteRecycler.RemoveRangeAtIndex(Start, End - Start + 1);
            }
            // Deletes and shrinks a random active entry.
            else if ((Input.GetKeyDown(KeyCode.R) || DemoToolbar.GetButtonDown(3)) && activeEntriesRange.HasValue)
            {
                int deletionIndex = Random.Range(activeEntriesRange.Value.Start, activeEntriesRange.Value.End);
                TestRecyclerEditorLogger.Log($"Deleting at {deletionIndex}");
                _deleteRecycler.GetActiveEntryWithIndex<DeleteRecyclerEntry>(deletionIndex).ShrinkAndDelete();
            }
        }

        private void OnValidate()
        {
            if (_deleteRecycler == null)
            {
                _deleteRecycler = GetComponent<DeleteRecyclerScrollRect>();
            }
        }
    }
}
