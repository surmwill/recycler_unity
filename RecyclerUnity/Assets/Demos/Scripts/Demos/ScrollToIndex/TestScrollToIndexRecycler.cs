using System.Linq;
using UnityEngine;

namespace Swill.Recycler.Demos
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
        
        private const float NormalScrollSpeed = 2.5f;
        private const float ScrollWhileGrowShrinkingSpeed = 0.5f;
        
        protected override RecyclerScrollRect<ScrollToIndexData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Scroll to index demo";

        protected override string DemoDescription => "Tests scrolling behaviour.";

        protected override string[] DemoButtonDescriptions => new[]
        {
            $"0 (keys 'A' and 'M'): Scrolls to the middle index {ScrollToMiddleIndex}.",
            $"1 (keys 'A' and 'T'): Scrolls to the top index 0.",
            $"2 (keys 'A' and 'B'): Scrolls to the bottom index {InitNumEntries - 1}.",

            $"3 (keys 'F' and 'G'): Scrolls to the middle index {ScrollToMiddleIndex} while making the bottom visible entry grow, scrolling over the expanding entry.",
            $"4 (keys 'F' and 'S'): Scrolls to the middle index {ScrollToMiddleIndex} while making the bottom visible entry shrink, scrolling over the shrinking entry.",
            
            $"5 (keys 'I' and 'M'): Scrolls immediately to the middle index {ScrollToMiddleIndex}.",
            $"6 (keys 'I' and 'T'): Scrolls immediately to the top index 0.",
            $"7 (keys 'I' and 'B'): Scrolls immediately to the bottom index {InitNumEntries - 1}.",
            
            $"8 (keys 'E' and 'T'): Scrolls immediately to the top edge of the middle index {ScrollToMiddleIndex}.",
            $"9 (keys 'E' and 'B'): Scrolls immediately to the bottom edge of the middle index {ScrollToMiddleIndex}.",

            $"10 (key 'C'): Cancels the current scroll call.",
            $"11 (key 'V'): Toggles the middle indicator on/off to know if we've properly centered on an index."
        };

        private IRecyclerScrollRectActiveEntriesWindow<ScrollToIndexData, string> _window;

        protected override void Start()
        {
            base.Start();
            _recycler.AppendEntries(Enumerable.Repeat((ScrollToIndexData) null, InitNumEntries).Select(_ => new ScrollToIndexData()));
            _window = _recycler.ActiveEntriesWindow;
        }

        private void Update()
        {
            /*** Animate scroll ***/
            // Scroll to middle index
            if ((Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.M)) || DemoToolbar.GetButtonDown(0))
            {
                _recycler.ScrollToIndex(ScrollToMiddleIndex, 
                    scrollSpeedViewportsPerSecond:NormalScrollSpeed,
                    onScrollComplete:() => TestRecyclerEditorLogger.Log("Middle index scroll complete."),
                    onScrollCancelled:() => TestRecyclerEditorLogger.Log("Middle index scroll cancelled."));
            }
            // Scroll to top index
            else if ((Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.T)) || DemoToolbar.GetButtonDown(1))
            {
                _recycler.ScrollToIndex(0,
                    scrollSpeedViewportsPerSecond:NormalScrollSpeed,
                    onScrollComplete:() => TestRecyclerEditorLogger.Log("Top index scroll complete."),
                    onScrollCancelled:() => TestRecyclerEditorLogger.Log("Top index scroll cancelled."));
            }
            // Scroll to bot index
            else if ((Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.B)) || DemoToolbar.GetButtonDown(2))
            {
                _recycler.ScrollToIndex(_recycler.DataForEntries.Count - 1, 
                    scrollSpeedViewportsPerSecond:NormalScrollSpeed,
                    onScrollComplete:() => TestRecyclerEditorLogger.Log("Bottom index scroll complete."),
                    onScrollCancelled:() => TestRecyclerEditorLogger.Log("Bottom index scroll cancelled."));
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

        private void OnValidate()
        {
            if (_recycler == null)
            {
                _recycler = GetComponent<ScrollToIndexRecyclerScrollRect>();
            }
        }
    }
}
