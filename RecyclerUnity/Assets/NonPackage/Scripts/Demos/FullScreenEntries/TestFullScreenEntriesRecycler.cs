using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Entry for testing a recycler with full screen entries and endcap.
    /// </summary>
    public class TestFullScreenEntriesRecycler : TestRecycler<EmptyRecyclerData, string>
    {
        [SerializeField]
        private EmptyRecyclerScrollRect _recycler = null;

        private const int InitNumEntries = 5;

        private const int NumEntriesToAppend = 3;

        protected override RecyclerScrollRect<EmptyRecyclerData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Full-screen entries demo.";

        protected override string DemoDescription => "Tests a recycler with full-screen entries and endcap.";

        protected override string[] DemoButtonDescriptions => new []
        {
            $"0: Appends {NumEntriesToAppend} entries.",
            $"1: Deletes the last entry.",
            $"2: Inserts an entry into a random index in the active entry window.",
            $"3: Deletes an entry at a random index in the active entry window.",
            $"4: Immediately scrolls to the top of topmost entry.",
            $"5: Immediately scrolls to the bottom of the bottommost entry."
        };

        private IRecyclerScrollRectActiveEntriesWindow _indexWindow;

        protected override void Start()
        {
            base.Start();
            _indexWindow = _recycler.ActiveEntriesWindow;
            _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitNumEntries));
        }

        private void Update()
        {
            (int Start, int End) = _indexWindow.ActiveEntriesRange.Value;
            
            // Inserts a random active entry
            if ((Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.A)) || DemoToolbar.GetButtonDown(2))
            {
                int insertionIndex = Random.Range(Start, End);
                Debug.Log($"Inserting at {insertionIndex}");
                
                _recycler.InsertAtIndex(insertionIndex, new EmptyRecyclerData(), FixEntries.Below);                
            }
            // Deletes a random active entry
            else if ((Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.D)) || DemoToolbar.GetButtonDown(3))
            {
                int deletionIndex = Random.Range(Start, End);
                Debug.Log($"Deleting at {deletionIndex}");
                
                _recycler.RemoveAtIndex(deletionIndex, FixEntries.Below);
            }
            
            // Appends entries
            else if (Input.GetKeyDown(KeyCode.A) || DemoToolbar.GetButtonDown(0))
            {
                _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(NumEntriesToAppend));
            }
            // Deletes the last entry
            else if (Input.GetKeyDown(KeyCode.D) || DemoToolbar.GetButtonDown(1))
            {
                _recycler.RemoveAtIndex(_recycler.DataForEntries.Count - 1, FixEntries.Below);
            }
            
            // Immediately scrolls to the top of the topmost entry
            else if ((Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.T)) || DemoToolbar.GetButtonDown(4))
            {
                _recycler.ScrollToIndexImmediate(0, ScrollToAlignment.EntryTop);
            }
            // Immediately scrolls to the bottom of the bottommost entry
            else if (Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.B) || DemoToolbar.GetButtonDown(5))
            {
                _recycler.ScrollToIndexImmediate(_recycler.DataForEntries.Count - 1, ScrollToAlignment.EntryBottom);
            }
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
