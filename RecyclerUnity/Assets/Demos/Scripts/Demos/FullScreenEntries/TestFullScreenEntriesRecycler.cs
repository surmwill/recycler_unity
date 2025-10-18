using UnityEngine;

namespace Swill.Recycler.Demos
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
            $"0 (key 'A'): Appends {NumEntriesToAppend} entries.",
            $"1 (key 'D'): Deletes the last entry.",
            $"2 (keys 'R' and 'A'): Inserts an entry into a random index in the active entry window.",
            $"3 (keys 'R' and 'D'): Deletes an entry at a random index in the active entry window.",
            $"4 (keys 'S' and 'T') Immediately scrolls to the top of topmost entry.",
            $"5 (keys 'S' and 'B') Immediately scrolls to the bottom of the bottommost entry."
        };

        private IRecyclerScrollRectActiveEntriesWindow<EmptyRecyclerData, string> _indexWindow;

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
                    Debug.Log($"Inserting at {insertionIndex}");
                
                    _recycler.InsertAtIndex(insertionIndex, new EmptyRecyclerData(), FixEntries.Below);
                    return;
                }
                
                // Deletes a random active entry
                if ((Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.D)) || DemoToolbar.GetButtonDown(3))
                {
                    int deletionIndex = Random.Range(Start, End);
                    Debug.Log($"Deleting at {deletionIndex}");
                
                    _recycler.RemoveAtIndex(deletionIndex, FixEntries.Below);
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
                _recycler.RemoveAtIndex(_recycler.DataForEntries.Count - 1, FixEntries.Below);
                return;
            }

            if (_recycler.DataForEntries.Count > 0)
            {
                // Immediately scrolls to the top of the topmost entry
                if ((Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.T)) || DemoToolbar.GetButtonDown(4))
                {
                    _recycler.ScrollToIndexImmediate(0, ScrollToAlignment.EntryTop);
                    return;
                }
                
                // Immediately scrolls to the bottom of the bottommost entry
                if (Input.GetKey(KeyCode.S) && Input.GetKeyDown(KeyCode.B) || DemoToolbar.GetButtonDown(5))
                {
                    _recycler.ScrollToIndexImmediate(_recycler.DataForEntries.Count - 1, ScrollToAlignment.EntryBottom);
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
