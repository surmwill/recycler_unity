using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests the recycler for animating an entry in on insertion/deletion.
    /// While there are already demos for insertion and deletion, this one is more polished and suitable for videos.
    /// </summary>
    public class TestPrettyInsertDeleteRecycler : TestRecycler<PrettyInsertDeleteData, string>
    {
        [SerializeField]
        private PrettyInsertDeleteRecycler _recycler = null;
        
        private const int InitNumEntries = 30;
        private const int NumEntriesInsertedAtMiddle = 4;
        
        private const int NumEntriesDeletedBeforeMiddle = 2;
        private const int NumEntriesDeletedAfterMiddle = 1;

        private RecyclerValidityChecker<PrettyInsertDeleteData, string> _validityChecker;

        protected override RecyclerScrollRect<PrettyInsertDeleteData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Pretty insertion and deletion demo";

        protected override string DemoDescription =>
            "Tests animating in/out entries on insertion/delete. " +
            "(While there are already demos for insertion and deletion, this one is more polished and suitable for videos.";

        protected override string[] DemoButtonDescriptions => new[]
        {
            "0: Inserts an entry at the end.",
            "1: Deletes the entry at the end.",
            $"2: Adds {NumEntriesInsertedAtMiddle} entries to the middle of wherever we are in the list.",
            $"3: Deletes {NumEntriesDeletedBeforeMiddle + 1 + NumEntriesDeletedAfterMiddle} entries from the middle of wherever we are in the list.",
        };

        protected override void Start()
        {
            base.Start();
            _recycler.AppendEntries(PrettyInsertDeleteData.GenerateData(InitNumEntries, false));
        }

        private void Update()
        {
            int dataLength = _recycler.DataForEntries.Count;
            (int visibleStartIndex, int visibleEndIndex) = _recycler.ActiveEntriesWindow.VisibleIndexRange.Value;
            int middleEntryIndex = visibleStartIndex + (visibleEndIndex - visibleStartIndex + 1) / 2;
            
            // Add entries at middle
            if ((Input.GetKey(KeyCode.M) && Input.GetKeyDown(KeyCode.A)) || DemoToolbar.GetButtonDown(2))
            {
                _recycler.InsertRangeAtIndex(middleEntryIndex, PrettyInsertDeleteData.GenerateData(NumEntriesInsertedAtMiddle, true, FixEntries.Mid));  
            }
            // Delete entries at middle
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
            // Add entry at bottom
            else if (Input.GetKeyDown(KeyCode.A) || DemoToolbar.GetButtonDown(0))
            {
                _recycler.InsertAtIndex(dataLength, new PrettyInsertDeleteData(true, FixEntries.Below));
            }
            // Delete entry at bottom
            else if (Input.GetKeyDown(KeyCode.D) || DemoToolbar.GetButtonDown(1))
            {
                for (int i = visibleEndIndex; i >= visibleStartIndex; i--)
                {
                    PrettyInsertDeleteEntry entry = (PrettyInsertDeleteEntry) _recycler.ActiveEntries[i];
                    if (!entry.IsDeleteing)
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
