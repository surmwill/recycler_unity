using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests a recycler working with auto-sized entries
    /// </summary>
    public class TestAutoSizeRecycler : TestRecycler<AutoSizeData, string>
    {
        [SerializeField]
        private AutoSizeRecycler _autoSizeRecycler = null;

        private const int NumEntries = 30;

        private const int MinNumLines = 1;
        private const int MaxNumLines = 6;
        
        protected override RecyclerScrollRect<AutoSizeData, string> ValidateRecycler => _autoSizeRecycler;

        protected override string DemoTitle => "Auto-size Demo";

        protected override string DemoDescription => "Tests auto-sized entries. Each entry is a different size.";

        protected override string[] DemoButtonDescriptions => new[]
        {
            "0: Appends a random number of lines of text to a random active entry.",
            "1: Increases the size of the endcap through its layout group.",
            "2: Decreases the size of the endcap through its layout group."
        };

        private IRecyclerScrollRectActiveEntriesWindow _indexWindow;

        protected override void Start()
        {
            base.Start();
            _autoSizeRecycler.AppendEntries(Enumerable.Range(0, NumEntries)
                .Select(_ => new AutoSizeData(Random.Range(MinNumLines, MaxNumLines + 1))));

            _indexWindow = _autoSizeRecycler.ActiveEntriesWindow;
        }

        private void Update()
        {
            (int Start, int End) = _indexWindow.ActiveEntriesRange.Value;
            
            // Randomly grow an active entry.
            if (Input.GetKeyDown(KeyCode.R) || DemoToolbar.GetButtonDown(0))
            {
                int appendTextToIndex = Random.Range(Start, End);
                Debug.Log($"Adding text to entry {appendTextToIndex}.");
                ((AutoSizeEntry) _autoSizeRecycler.ActiveEntries[appendTextToIndex]).AppendLines();
            }
            // Increases the size of the endcap through its layout group.
            else if (Input.GetKeyDown(KeyCode.A) || DemoToolbar.GetButtonDown(1))
            {
                ((AutoSizeEndcap) _autoSizeRecycler.Endcap).Grow();
            }
            // Decreases the size of the endcap through its layout group.
            else if (Input.GetKeyDown(KeyCode.D) || DemoToolbar.GetButtonDown(2))
            {
                ((AutoSizeEndcap) _autoSizeRecycler.Endcap).Shrink();
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
