using System.Linq;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Demos deleting a couple entries in the recycler.
    /// </summary>
    public class TestDeleteRecyclerScrollRect : TestRecycler<EmptyRecyclerData, string>
    {
        [SerializeField]
        private DeleteRecyclerScrollRect _deleteRecycler = null;

        private const int InitNumEntries = 50;

        public const int DeleteAtIndex = 15;
        private const int NumEntriesToDelete = 3;

        protected override RecyclerScrollRect<EmptyRecyclerData, string> ValidateRecycler => _deleteRecycler;

        protected override string DemoTitle => "Deletion Demo";

        protected override string DemoDescription => $"Tests deletion of entries.";

        protected override string[] DemoButtonDescriptions => new[]
        {
            $"0: Shrinks and deletes {NumEntriesToDelete} starting at {DeleteAtIndex}.",
            $"1: Batch deletes the last {NumEntriesToDelete} instantly."
        };

        protected override void Start()
        {
            base.Start();
            _deleteRecycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitNumEntries));
        }

        private void Update()
        {
            // Animate delete
            if (Input.GetKeyDown(KeyCode.A) || DemoToolbar.GetButtonDown(0))
            {
                string[] deleteKeys = Enumerable.Range(DeleteAtIndex, NumEntriesToDelete).Select(i => _deleteRecycler.GetKeyForCurrentIndex(i)).ToArray();
                foreach (string key in deleteKeys)
                {
                    if (_deleteRecycler.ActiveEntries.TryGetValue(_deleteRecycler.GetCurrentIndexForKey(key), out RecyclerScrollRectEntry<EmptyRecyclerData, string> entry))
                    {
                        ((DeleteRecyclerEntry) entry).ShrinkAndDelete();
                    }
                    else
                    {
                        _deleteRecycler.RemoveAtKey(key);
                    }
                }
            }
            // Immediate batch delete
            else if (Input.GetKeyDown(KeyCode.D) || DemoToolbar.GetButtonDown(1))
            { 
                _deleteRecycler.RemoveRangeAtIndex(_deleteRecycler.DataForEntries.Count - NumEntriesToDelete, NumEntriesToDelete, FixEntries.Below);
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
