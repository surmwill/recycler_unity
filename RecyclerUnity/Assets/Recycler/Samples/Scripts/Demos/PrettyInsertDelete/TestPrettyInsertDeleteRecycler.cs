using UnityEngine;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Tests the recycler for animating an entry in on insertion/deletion.
    /// While there are already demos for insertion and deletion, this one is more polished and suitable for videos.
    /// </summary>
    public class TestPrettyInsertDeleteRecycler : TestRecycler<string, PrettyInsertDeleteData>
    {
        [SerializeField]
        private PrettyInsertDeleteRecycler _recycler = null;
        
        private const int InitNumEntries = 30;
        private const int NumEntriesInsertedAtMiddle = 4;
        
        private const int NumEntriesDeletedBeforeMiddle = 2;
        private const int NumEntriesDeletedAfterMiddle = 1;

        private RecyclerValidityChecker<string, PrettyInsertDeleteData> _validityChecker;

        protected override RecyclerScrollRect<string, PrettyInsertDeleteData> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Pretty insertion and deletion demo";

        protected override string DemoDescription =>
            "Tests animating in/out entries on insertion/delete. " +
            "(While there are already demos for insertion and deletion, this one is more polished and suitable for videos.";

        protected override string[] DemoButtonDescriptions => new[]
        {
            "0 (key 'A'): Inserts an entry at the end of the list.",
            "1 (key 'D'): Deletes the first entry starting at the end of the visible window that is not currently in the process of being deleted.",
            $"2 (keys 'M' and then 'A'): Adds {NumEntriesInsertedAtMiddle} entries to the middle of the visible window.",
            $"3 (keys 'M' and then 'D'): Deletes {NumEntriesDeletedBeforeMiddle + 1 + NumEntriesDeletedAfterMiddle} entries from the middle of the visible window.",
        };

        protected override void Start()
        {
            base.Start();
            _recycler.AppendEntries(PrettyInsertDeleteData.GenerateData(InitNumEntries, false));
        }

        private void Update()
        {
            // Inserts an entry at the end of the list
            if ((!Input.GetKey(KeyCode.M) && Input.GetKeyDown(KeyCode.A)) || DemoToolbar.GetButtonDown(0))
            {
                _recycler.InsertAtIndex(_recycler.DataForEntries.Count, new PrettyInsertDeleteData(true, FixEntries.Below));
                return;
            }

            if (!_recycler.ActiveEntriesWindow.VisibleIndexRange.HasValue)
            {
                return;
            }
            
            (int visibleStartIndex, int visibleEndIndex) = _recycler.ActiveEntriesWindow.VisibleIndexRange.Value;
            int middleEntryIndex = visibleStartIndex + (visibleEndIndex - visibleStartIndex + 1) / 2;
            
            // Add entries at middle of the visible window
            if ((Input.GetKey(KeyCode.M) && Input.GetKeyDown(KeyCode.A)) || DemoToolbar.GetButtonDown(2))
            {
                _recycler.InsertRangeAtIndex(middleEntryIndex, PrettyInsertDeleteData.GenerateData(NumEntriesInsertedAtMiddle, true, FixEntries.Mid));  
            }
            // Delete entries in the middle of the visible window 
            else if ((Input.GetKey(KeyCode.M) && Input.GetKeyDown(KeyCode.D)) || DemoToolbar.GetButtonDown(3))
            {
                int startDeleteIndex = middleEntryIndex - NumEntriesDeletedBeforeMiddle;
                int endDeleteIndex = middleEntryIndex + NumEntriesDeletedAfterMiddle;
                
                for (int i = startDeleteIndex; i <= endDeleteIndex; i++)
                {
                    PrettyInsertDeleteEntry entry = (PrettyInsertDeleteEntry) _recycler.ActiveEntries[i];
                    entry.AnimateOutAndDelete(FixEntries.Mid);
                }
            }
            // Deletes the first entry starting at the end of the visible window that is not currently in the process of being deleted
            else if (Input.GetKeyDown(KeyCode.D) || DemoToolbar.GetButtonDown(1))
            {
                for (int i = visibleEndIndex; i >= visibleStartIndex; i--)
                {
                    PrettyInsertDeleteEntry entry = (PrettyInsertDeleteEntry) _recycler.ActiveEntries[i];
                    if (!entry.IsDeleting)
                    {
                        entry.AnimateOutAndDelete(FixEntries.Below);
                        break;
                    }
                }
            }
        }

        private void OnValidate()
        {
            if (_recycler == null)
            {
                _recycler = GetComponent<PrettyInsertDeleteRecycler>();
            }
        }
    }
}
