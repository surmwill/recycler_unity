using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Demos scrolling to an index in a recycler
    /// </summary>
    public class TestScrollToIndexRecycler : TestRecycler<ScrollToIndexData, string>
    {
        [SerializeField]
        private ScrollToIndexRecyclerScrollRect _recycler = null;

        [SerializeField]
        private GameObject _middleIndicator = null;

        private const int InitNumEntries = 50;
        private const int ScrollToMiddleIndex = 25;
        private const float ScrollWhileGrowShrinkingSpeed = 0.5f;

        private static readonly int[] EnlargeEntryIndices = { 41, 42 };

        protected override RecyclerScrollRect<ScrollToIndexData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Scroll to index demo";

        protected override string DemoDescription => "Tests scrolling behaviour.";

        protected override string[] DemoButtonDescriptions => new[]
        {
            $"0: Scrolls to the middle index {ScrollToMiddleIndex}.",
            $"1: Scrolls to the top index 0.",
            $"2: Scrolls to the bottom index {InitNumEntries - 1}.",

            $"3: Scrolls to the middle index {ScrollToMiddleIndex} while making the bottom visible entry grow, scrolling over the expanding entry.",
            $"4: Scrolls to the middle index {ScrollToMiddleIndex} while making the bottom visible entry shrink, scrolling over the shrinking entry.",
            
            $"5: Scrolls immediately to the middle index {ScrollToMiddleIndex}.",
            $"6: Scrolls immediately to the top index 0.",
            $"7: Scrolls immediately to the bottom index {InitNumEntries - 1}.",
            
            $"8: Scrolls immediately to the top edge of the middle index {ScrollToMiddleIndex}.",
            $"9: Scrolls immediately to the bottom edge of the middle index {ScrollToMiddleIndex}.",

            $"10: Cancels the current scroll call.",
            $"11: Toggles the middle indicator on/off to know if we've properly centered on an index."
        };

        private IRecyclerScrollRectActiveEntriesWindow _window;

        protected override void Start()
        {
            base.Start();
            _recycler.AppendEntries(CreateEntryData(InitNumEntries, EnlargeEntryIndices));
            _window = _recycler.ActiveEntriesWindow;
        }

        private void Update()
        {
            /*** Animate scroll ***/
            // Scroll to middle index
            if ((Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.M)) || DemoToolbar.GetButtonDown(0))
            {
                _recycler.ScrollToIndex(ScrollToMiddleIndex, 
                    onScrollComplete:() => Debug.Log("Middle index scroll complete."),
                    onScrollCancelled:() => Debug.Log("Middle index scroll cancelled."));
            }
            // Scroll to top index
            else if ((Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.T)) || DemoToolbar.GetButtonDown(1))
            {
                _recycler.ScrollToIndex(0, 
                    onScrollComplete:() => Debug.Log("Top index scroll complete."),
                    onScrollCancelled:() => Debug.Log("Top index scroll cancelled."));
            }
            // Scroll to bot index
            else if ((Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.B)) || DemoToolbar.GetButtonDown(2))
            {
                _recycler.ScrollToIndex(_recycler.DataForEntries.Count - 1, 
                    onScrollComplete:() => Debug.Log("Bottom index scroll complete."),
                    onScrollCancelled:() => Debug.Log("Bottom index scroll cancelled."));
            }

            /*** Fighting ***/
            // Scroll to the middle while making the bottom visible entry grow, scrolling over the expanding entry
            else if ((Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.G)) || DemoToolbar.GetButtonDown(3))
            {
                _recycler.ScrollToIndex(ScrollToMiddleIndex, scrollSpeedViewportsPerSecond:ScrollWhileGrowShrinkingSpeed);
                ((ScrollToIndexRecyclerScrollRectEntry) _recycler.ActiveEntries[_window.VisibleIndexRange.Value.End]).Grow(FixEntries.Above);
            }
            // Scroll to the middle while making the bottom visible entry shrink, scrolling over the shrinking entry
            else if ((Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.S)) || DemoToolbar.GetButtonDown(4))
            {
                _recycler.ScrollToIndex(ScrollToMiddleIndex, scrollSpeedViewportsPerSecond:ScrollWhileGrowShrinkingSpeed);
                ((ScrollToIndexRecyclerScrollRectEntry) _recycler.ActiveEntries[_window.VisibleIndexRange.Value.End]).Shrink(FixEntries.Above);
            }

            /*** Immediate Scroll ***/
            // Scroll immediate to middle index
            else if ((Input.GetKey(KeyCode.I) && Input.GetKeyDown(KeyCode.M)) || DemoToolbar.GetButtonDown(5))
            {
                _recycler.ScrollToIndexImmediate(ScrollToMiddleIndex);
            }
            // Scroll immediate to top index
            else if ((Input.GetKey(KeyCode.I) && Input.GetKeyDown(KeyCode.T)) || DemoToolbar.GetButtonDown(6))
            {
                _recycler.ScrollToIndexImmediate(0);
            }
            // Scroll immediate to bot index
            else if ((Input.GetKey(KeyCode.I) && Input.GetKeyDown(KeyCode.B)) || DemoToolbar.GetButtonDown(7))
            {
                _recycler.ScrollToIndexImmediate(_recycler.DataForEntries.Count - 1);
            }
            
            /*** Edges ***/
            // Scroll immediate top edge
            else if ((Input.GetKey(KeyCode.E) && Input.GetKeyDown(KeyCode.T)) || DemoToolbar.GetButtonDown(8))
            {
                _recycler.ScrollToIndexImmediate(ScrollToMiddleIndex, ScrollToAlignment.EntryTop);
            }
            // Scroll immediate bottom edge
            else if ((Input.GetKey(KeyCode.E) && Input.GetKeyDown(KeyCode.B)) || DemoToolbar.GetButtonDown(9))
            {
                _recycler.ScrollToIndexImmediate(ScrollToMiddleIndex, ScrollToAlignment.EntryBottom);
            }
            
            /*** Other ***/
            // Test cancel scroll to
            else if (Input.GetKeyDown(KeyCode.C) || DemoToolbar.GetButtonDown(10))
            {
                _recycler.CancelScrollTo();
            }
            // Toggle the middle indicator
            else if (Input.GetKeyDown(KeyCode.V) || DemoToolbar.GetButtonDown(11))
            {
                _middleIndicator.SetActive(!_middleIndicator.activeSelf);
            }
        }

        private ScrollToIndexData[] CreateEntryData(int numEntries, IEnumerable<int> enlargeIndices = null)
        {
            HashSet<int> enlarge = new HashSet<int>(enlargeIndices ?? Array.Empty<int>());
            return Enumerable.Repeat((ScrollToIndexData) null, numEntries)
                .Select((_, i) => new ScrollToIndexData(enlarge.Contains(i)))
                .ToArray();
        }

        private void OnValidate()
        {
            if (_recycler == null)
            {
                _recycler = GetComponent<ScrollToIndexRecyclerScrollRect>();
            }
        }
    }
}
