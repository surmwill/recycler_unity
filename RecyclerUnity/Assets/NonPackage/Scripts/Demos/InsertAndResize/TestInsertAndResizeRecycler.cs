using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests inserting entries into the recycler.
    /// </summary>
    public class TestInsertAndResizeRecycler : TestRecycler<InsertAndResizeData, string>
    {
        [SerializeField]
        private InsertAndResizeRecycler _recycler = null;

        private const int InitNumEntries = 30;
        private const int InsertionIndex = 15;
        private const int NumInsertionEntries = 3;
        private const int MoreThanFullScreenNumEntries = 20;

        protected override RecyclerScrollRect<InsertAndResizeData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Insert and resize demo";

        protected override string DemoDescription => "Tests inserting new entries into the recycler.";

        protected override string[] DemoButtonDescriptions => new[]
        {
            $"0: Inserts and grows {NumInsertionEntries} entries at index {InsertionIndex}",
            $"1: Batch inserts {NumInsertionEntries} to the end of the list.",
            $"2: Batch inserts a fullscreen's worth of entries {MoreThanFullScreenNumEntries} to the end of the list.",
        };

        private IRecyclerScrollRectActiveEntriesWindow _activeEntriesWindow;

        protected override void Start()
        {
            base.Start();
            _recycler.AppendEntries(CreateDataForEntries(InitNumEntries, false));
            _activeEntriesWindow = _recycler.ActiveEntriesWindow;
        }
        
        private void Update()
        {
            (int Start, int End) = _activeEntriesWindow.ActiveEntriesRange.Value;
            
            // Inserts and grows entries.
            if (Input.GetKeyDown(KeyCode.A) || DemoToolbar.GetButtonDown(0))
            {
                _recycler.InsertRangeAtIndex(InsertionIndex, CreateDataForEntries(NumInsertionEntries, true));
            }
            // Immediately inserts a batch of entries at the end
            else if (Input.GetKeyDown(KeyCode.D) || DemoToolbar.GetButtonDown(1))
            {
                _recycler.InsertRangeAtIndex(_recycler.DataForEntries.Count - 1, CreateDataForEntries(NumInsertionEntries, false), FixEntries.Above);
            }
            // Immediately inserts a full screen of entries at the end
            else if (Input.GetKeyDown(KeyCode.F) || DemoToolbar.GetButtonDown(2))
            {
                _recycler.InsertRangeAtIndex(_recycler.DataForEntries.Count - 1, CreateDataForEntries(MoreThanFullScreenNumEntries, false), FixEntries.Above);
            }
            else if ((Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.A)) || DemoToolbar.GetButtonDown(3))
            {
                int insertionIndex = Random.Range(Start, End);
                Debug.Log($"Inserting at {insertionIndex}");
                
                _recycler.InsertAtIndex(insertionIndex, new InsertAndResizeData(true), FixEntries.Above);
            }
        }

        private IEnumerable<InsertAndResizeData> CreateDataForEntries(int count, bool shouldGrow)
        {
            return Enumerable.Repeat<object>(null, count).Select(_ => new InsertAndResizeData(shouldGrow));
        }

        private void OnValidate()
        {
            if (_recycler == null)
            {
                _recycler = GetComponent<InsertAndResizeRecycler>();
            }
        }
    }
}
