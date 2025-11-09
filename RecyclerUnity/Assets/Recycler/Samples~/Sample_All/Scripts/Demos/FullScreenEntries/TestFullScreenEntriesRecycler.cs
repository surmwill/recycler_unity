using UnityEngine;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Entry for testing a recycler with full screen entries and endcap.
    /// </summary>
    public class TestFullScreenEntriesRecycler : TestRecycler<string, EmptyRecyclerData>
    {
        [SerializeField]
        private EmptyRecyclerScrollRect _recycler = null;

        private const int InitNumEntries = 5;

        private const int NumEntriesToAppend = 3;

        protected override RecyclerScrollRect<string, EmptyRecyclerData> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Full-screen entries demo.";

        protected override string DemoDescription => "Tests a recycler with full-screen entries and endcap.";

        protected override string[] DemoButtonDescriptions => new []
        {
            $"0 (key 'A'): Appends {NumEntriesToAppend} entries.",
            $"1 (key 'D'): Deletes the last entry.",
            $"2 (keys 'R' and then 'A'): Inserts an entry into a random index in the active entry window.",
            $"3 (keys 'R' and then 'D'): Deletes an entry at a random index in the active entry window.",
            $"4 (keys 'S' and then 'T') Immediately scrolls to the top/left of the starting entry.",
            $"5 (keys 'S' and then 'B') Immediately scrolls to the bottom/right of the ending entry."
        };

        private IRecyclerScrollRectActiveEntriesWindow<string, EmptyRecyclerData> _indexWindow;

        protected override void Start()
        {
            base.Start();
            _indexWindow = _recycler.ActiveEntriesWindow;
            _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitNumEntries));
        }
    
        private void Update()
        {
            // Inserts a random active entry
            if (_indexWindow.ActiveEntriesRange.HasValue)
            {
                 (int Start, int End) = _indexWindow.ActiveEntriesRange.Value;
                
                if ((Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.A)) || DemoToolbar.GetButtonDown(2))
                {
                    int insertionIndex = Random.Range(Start, End);
                    TestRecyclerEditorLogger.Log($"Inserting at {insertionIndex}");
                
                    _recycler.InsertAtIndex(insertionIndex, new EmptyRecyclerData(), _recycler.Orientation.IsVertical() ? FixEntries.VerticalAbove : FixEntries.HorizontalLeft);
                    return;
                }
                
                // Deletes a random active entry
                if ((Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.D)) || DemoToolbar.GetButtonDown(3))
                {
                    int deletionIndex = Random.Range(Start, End);
                    TestRecyclerEditorLogger.Log($"Deleting at {deletionIndex}");
                
                    _recycler.RemoveAtIndex(deletionIndex, _recycler.Orientation.IsVertical() ? FixEntries.VerticalAbove : FixEntries.HorizontalLeft);
                    return;
                }   
            }
            
            // Appends entries
            if (Input.GetKeyDown(KeyCode.A) || DemoToolbar.GetButtonDown(0))
            {
                _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(NumEntriesToAppend));
                return;
            }
            
            // Deletes the last entry
            if (Input.GetKeyDown(KeyCode.D) || DemoToolbar.GetButtonDown(1))
            {
                _recycler.RemoveAtIndex(_recycler.DataForEntries.Count - 1, _recycler.Orientation.IsVertical() ? FixEntries.VerticalAbove : FixEntries.VerticalBelow);
                return;
            }

            if (_recycler.DataForEntries.Count > 0)
            {
                // Immediately scrolls to the top/left of the starting entry
                if ((Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.T)) || DemoToolbar.GetButtonDown(4))
                {
                    _recycler.ScrollToIndexImmediate(0, _recycler.Orientation.IsVertical() ? ScrollToAlignment.VerticalEntryTop : ScrollToAlignment.HorizontalEntryLeft);
                    return;
                }
                
                // Immediately scrolls to the bottom/right of the ending entry
                if (Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.B) || DemoToolbar.GetButtonDown(5))
                {
                    _recycler.ScrollToIndexImmediate(_recycler.DataForEntries.Count - 1, _recycler.Orientation.IsVertical() ? ScrollToAlignment.VerticalEntryBottom : ScrollToAlignment.HorizontalEntryRight);
                    return;
                }   
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
