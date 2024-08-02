using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests clearing and adding entries to a recycler, one-by-one
    /// </summary>
    public class TestClearAndFillRecycler : TestRecycler<EmptyRecyclerData, string>
    {
        [SerializeField]
        private EmptyRecyclerScrollRect _recycler = null;

        protected override RecyclerScrollRect<EmptyRecyclerData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Clear and fill demo";

        protected override string DemoDescription => "Tests clearing entries and inserting them one-by-one.";

        protected override string[] DemoButtonDescriptions => new[]
        {
            "0: Appends a new entry.",
            "1: Deletes the first entry",
            "2: Clears the entries",
            "3: Resets the list to the beginning entries."
        };

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A) || DemoToolbar.GetButtonDown(0))
            {
                _recycler.AppendEntries(new[] { new EmptyRecyclerData() });
            }
            else if ((Input.GetKeyDown(KeyCode.D) || DemoToolbar.GetButtonDown(1)) && _recycler.DataForEntries.Count > 0)
            {
                _recycler.RemoveAtIndex(0);
            }
            else if (Input.GetKeyDown(KeyCode.C) || DemoToolbar.GetButtonDown(2))
            {
                ClearAndCheck();
            }
            else if (Input.GetKeyDown(KeyCode.R) || DemoToolbar.GetButtonDown(3))
            {
                _recycler.ResetToBeginning();
            }
        }

        private void ClearAndCheck()
        {
            // Private fields in the RecyclerScrollRect that we'd like access here for testing purposes, matching their name 1-to-1
            RecycledEntries<EmptyRecyclerData, string> _recycledEntries = null;
            Queue<RecyclerScrollRectEntry<EmptyRecyclerData, string>> _unboundEntries = null;
            
            Dictionary<string, int> _entryKeyToCurrentIndex = null;
            RecyclerScrollRectActiveEntriesWindow _activeEntriesWindow = null;
            
            int? _currScrollingToIndex = null;
            Coroutine _scrollToIndexCoroutine = null;
            Vector2 _nonFilledScrollRectPivot = default;
            
            _recycledEntries = GetRecyclerPrivateFieldValue<RecycledEntries<EmptyRecyclerData, string>>(nameof(_recycledEntries));
            _unboundEntries = GetRecyclerPrivateFieldValue<Queue<RecyclerScrollRectEntry<EmptyRecyclerData, string>>>(nameof(_unboundEntries));
            
            _entryKeyToCurrentIndex = GetRecyclerPrivateFieldValue<Dictionary<string, int>>(nameof(_entryKeyToCurrentIndex));
            _activeEntriesWindow = GetRecyclerPrivateFieldValue<RecyclerScrollRectActiveEntriesWindow>(nameof(_activeEntriesWindow));
            
            _currScrollingToIndex = GetRecyclerPrivateFieldValue<int?>(nameof(_currScrollingToIndex));
            _scrollToIndexCoroutine = GetRecyclerPrivateFieldValue<Coroutine>(nameof(_scrollToIndexCoroutine));
            _nonFilledScrollRectPivot = GetRecyclerPrivateFieldValue<Vector2>(nameof(_nonFilledScrollRectPivot));

            // Upon clearing, all entries should return to the pool unbound. We expect (and will check for) this amount of unbound entries
            int numTotalBoundEntries = _recycler.ActiveEntries.Count + _recycledEntries.Entries.Count;
            int numTargetUnboundEntries = numTotalBoundEntries + _unboundEntries.Count;

            // Actual clearing
            _recycler.Clear();
            
            // Ensure clearing resets us back to the recycler's initial state
            if (_recycler.DataForEntries.Any())
            {
                throw new DataException("The data is supposed to cleared, but there is still some present.");
            }

            if (_entryKeyToCurrentIndex.Any())
            {
                throw new DataException("The data has been cleared. There should be no keys either.");
            }

            if (_recycler.ActiveEntries.Any())
            {
                throw new DataException($"The data has been cleared. We should not have any active entries.");
            }
            
            if (_activeEntriesWindow.Exists || 
                _activeEntriesWindow.IsDirty || 
                _activeEntriesWindow.ActiveEntriesRange.HasValue || 
                _activeEntriesWindow.VisibleIndexRange.HasValue ||
                _activeEntriesWindow.StartCacheIndexRange.HasValue || 
                _activeEntriesWindow.EndCacheIndexRange.HasValue)
            {
                throw new DataException($"The data has been cleared and the window should not exist. There's no underlying data to have a window over.");
            }

            if (_recycledEntries.Entries.Any())
            {
                throw new DataException($"After clearing, all entries should return to the pool unbound. There are still {_recycledEntries.Entries.Count} entries in the pool bound.");
            }

            int numMissingUnboundEntries = numTargetUnboundEntries - _unboundEntries.Count;
            if (numMissingUnboundEntries != 0)
            {
                throw new DataException($"After clearing, all entries should return to the pool unbound. Missing {numMissingUnboundEntries} entries.");
            }

            if (_recycler.Endcap != null && _recycler.Endcap.gameObject.activeSelf)
            {
                throw new DataException("The data has been cleared. We expect an empty window and therefore the endcap should not be active.");
            }

            if (_currScrollingToIndex.HasValue || _scrollToIndexCoroutine != null)
            {
                throw new DataException("The data has been cleared. We should not be auto-scrolling to an index.");
            }

            if (_recycler.content.pivot != _nonFilledScrollRectPivot)
            {
                throw new DataException("After clearing, the pivot should be reset to whatever it was on initialization.");
            }
        }

        private TFieldValue GetRecyclerPrivateFieldValue<TFieldValue>(string fieldName)
        {
            return RecyclerScrollRectReflectionHelpers.GetPrivateFieldValue<TFieldValue, EmptyRecyclerData, string>(_recycler, fieldName);
        }

        private void OnValidate()
        {
            if (_recycler == null)
            {
                _recycler = GetComponent<EmptyRecyclerScrollRect>();
            }
        }
    }
}
