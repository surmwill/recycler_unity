using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Tests a recycler working with auto-sized entries
    /// </summary>
    public class TestAutoSizeRecycler : TestRecycler<string, AutoSizeData>
    {
        [SerializeField]
        private AutoSizeRecycler _autoSizeRecycler = null;

        [SerializeField]
        private bool _testVertical = false;

        private const int NumEntries = 30;

        private const int MinNumLines = 1;
        private const int MaxNumLines = 6;

        private bool IsValid => _testVertical == _autoSizeRecycler.Orientation.IsVertical();
        
        protected override RecyclerScrollRect<string, AutoSizeData> ValidateRecycler => _autoSizeRecycler;

        protected override string DemoTitle => "Auto-size Demo";

        protected override string DemoDescription => "Tests auto-sized entries. Each entry holds a different amount of text and therefore has a different size. " +
                                                     "Buttons are used to dynamically append text to entries.";

        protected override string[] DemoButtonDescriptions => new[]
        {
            "0 (or 'A'): Appends a random number of lines of text to a random active entry.",
            "1 (or 'S'): Increases the size of the endcap through its layout group.",
            "2 (or 'D'): Decreases the size of the endcap through its layout group."
        };

        private IRecyclerScrollRectActiveEntriesWindow<string, AutoSizeData> _indexWindow;

        protected override void Start()
        {
            if (!IsValid)
            {
                TestRecyclerEditorLogger.LogError($"This demo scenes requires a {(_testVertical ? "vertically" : "horizontally")} oriented recycler");
                return;
            }
            
            base.Start();
            _autoSizeRecycler.AppendEntries(Enumerable.Range(0, NumEntries).Select(_ => new AutoSizeData(Random.Range(MinNumLines, MaxNumLines + 1))));
            _indexWindow = _autoSizeRecycler.ActiveEntriesWindow;   
        }

        private void Update()
        {
            if (!IsValid)
            {
                return;
            }
            
            (int Start, int End) = _indexWindow.ActiveEntriesRange.Value;
            
            // Randomly grow an active entry.
            if (Input.GetKeyDown(KeyCode.A) || DemoToolbar.GetButtonDown(0))
            {
                int indexToAppend = Random.Range(Start, End);
                TestRecyclerEditorLogger.Log($"Appending text to: {indexToAppend}");
                _autoSizeRecycler.GetActiveEntryWithIndex<AutoSizeEntry>(indexToAppend).AppendLines();
            }
            // Increases the size of the endcap through its layout group.
            else if (Input.GetKeyDown(KeyCode.G) || DemoToolbar.GetButtonDown(1))
            {
                _autoSizeRecycler.GetEndcap<AutoSizeEndcap>().Grow();
            }
            // Decreases the size of the endcap through its layout group.
            else if (Input.GetKeyDown(KeyCode.S) || DemoToolbar.GetButtonDown(2))
            {
                _autoSizeRecycler.GetEndcap<AutoSizeEndcap>().Shrink();
            }
        }

        private void OnValidate()
        {
            if (_autoSizeRecycler == null)
            {
                _autoSizeRecycler = GetComponent<AutoSizeRecycler>();
            }
        }
    }
}
