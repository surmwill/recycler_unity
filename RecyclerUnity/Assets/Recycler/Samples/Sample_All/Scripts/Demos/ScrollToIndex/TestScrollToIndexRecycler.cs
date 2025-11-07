using System.Linq;
using UnityEngine;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Demos scrolling to an index in a recycler
    /// </summary>
    public class TestScrollToIndexRecycler : TestRecycler<string, ScrollToIndexData>
    {
        [SerializeField]
        private ScrollToIndexRecyclerScrollRect _recycler = null;

        [SerializeField]
        private GameObject _middleIndicator = null;

        private const int InitNumEntries = 50;
        private const int ScrollToMiddleIndex = 25;
        
        private const float NormalScrollSpeed = 2.5f;
        private const float ScrollWhileGrowShrinkingSpeed = 0.5f;
        
        protected override RecyclerScrollRect<string, ScrollToIndexData> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Scroll to index demo";

        protected override string DemoDescription => "Tests scrolling behaviour.";

        protected override string[] DemoButtonDescriptions => new[]
        {
            $"0 (keys 'A' and then 'M'): Scrolls to the middle of index {ScrollToMiddleIndex}.",
            $"1 (keys 'A' and then 'T'): Scrolls to the top/left of index 0.",
            $"2 (keys 'A' and then 'B'): Scrolls to the bottom/right of index {InitNumEntries - 1}.",

            $"3 (keys 'F' and then 'G'): Scrolls to the middle index {ScrollToMiddleIndex} while making an intermediate entry grow, scrolling over the expanding entry.",
            $"4 (keys 'F' and then 'S'): Scrolls to the middle index {ScrollToMiddleIndex} while making the intermediate entry shrink, scrolling over the shrinking entry.",
            
            $"5 (keys 'I' and then 'M'): Scrolls immediately to the top/left of the middle index {ScrollToMiddleIndex}.",
            $"6 (keys 'I' and then 'T'): Scrolls immediately to the top index 0.",
            $"7 (keys 'I' and then 'B'): Scrolls immediately to the bottom index {InitNumEntries - 1}.",
            
            $"8 (keys 'E' and then 'T'): Scrolls immediately to the top edge of the middle index {ScrollToMiddleIndex}.",
            $"9 (keys 'E' and then 'B'): Scrolls immediately to the bottom edge of the middle index {ScrollToMiddleIndex}.",

            $"10 (key 'C'): Cancels the current scroll call.",
            $"11 (key 'V'): Toggles the middle indicator on/off to know if we've properly centered on an index."
        };

        private IRecyclerScrollRectActiveEntriesWindow<string, ScrollToIndexData> _window;

        protected override void Start()
        {
            base.Start();
            _recycler.AppendEntries(Enumerable.Repeat((ScrollToIndexData) null, InitNumEntries).Select(_ => new ScrollToIndexData()));
            _window = _recycler.ActiveEntriesWindow;
        }

        private void Update()
        {
            /*** Animate scroll ***/
            // Scroll to middle of index
            if ((Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.M)) || DemoToolbar.GetButtonDown(0))
            {
                _recycler.ScrollToIndex(ScrollToMiddleIndex, 
                    scrollSpeedViewportsPerSecond:NormalScrollSpeed,
                    onScrollComplete:() => TestRecyclerEditorLogger.Log("Middle index scroll complete."),
                    onScrollCancelled:() => TestRecyclerEditorLogger.Log("Middle index scroll cancelled."));
            }
            // Scroll to top/left of index
            else if ((Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.T)) || DemoToolbar.GetButtonDown(1))
            {
                _recycler.ScrollToIndex(0,
                    _recycler.Orientation.IsVertical() ? ScrollToAlignment.VerticalEntryTop : ScrollToAlignment.HorizontalEntryLeft,
                    scrollSpeedViewportsPerSecond:NormalScrollSpeed,
                    onScrollComplete:() => TestRecyclerEditorLogger.Log("Top/left index scroll complete."),
                    onScrollCancelled:() => TestRecyclerEditorLogger.Log("Top/left index scroll cancelled."));
            }
            // Scroll to bot/right of index
            else if ((Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.B)) || DemoToolbar.GetButtonDown(2))
            {
                _recycler.ScrollToIndex(_recycler.DataForEntries.Count - 1, 
                    _recycler.Orientation.IsVertical() ? ScrollToAlignment.VerticalEntryBottom : ScrollToAlignment.HorizontalEntryRight,
                    scrollSpeedViewportsPerSecond:NormalScrollSpeed,
                    onScrollComplete:() => TestRecyclerEditorLogger.Log("Bottom/right index scroll complete."),
                    onScrollCancelled:() => TestRecyclerEditorLogger.Log("Bottom/right index scroll cancelled."));
            }

            /*** Fighting ***/
            // Scroll to the middle while making the bottom visible entry grow, scrolling over the expanding entry
            else if ((Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.G)) || DemoToolbar.GetButtonDown(3))
            {
                _recycler.ScrollToIndex(ScrollToMiddleIndex, scrollSpeedViewportsPerSecond:ScrollWhileGrowShrinkingSpeed);
                
                if (!_window.Contains(ScrollToMiddleIndex))
                {
                    (int Start, int End) = _window.VisibleIndexRange.Value;
                    ((ScrollToIndexRecyclerScrollRectEntry) _recycler.ActiveEntries[ScrollToMiddleIndex > End ? End : Start]).Grow(GrowShrinkInSameDirectionAsScrollToMiddle());   
                }
                else
                {
                    TestRecyclerEditorLogger.LogWarning($"Cannot test this behaviour while the middle entry {ScrollToMiddleIndex} is currently visible.");
                }
            }
            // Scroll to the middle while making the bottom visible entry shrink, scrolling over the shrinking entry
            else if ((Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.S)) || DemoToolbar.GetButtonDown(4))
            {
                _recycler.ScrollToIndex(ScrollToMiddleIndex, scrollSpeedViewportsPerSecond:ScrollWhileGrowShrinkingSpeed);
                
                if (!_window.Contains(ScrollToMiddleIndex))
                {
                    (int Start, int End) = _window.VisibleIndexRange.Value;
                    ((ScrollToIndexRecyclerScrollRectEntry) _recycler.ActiveEntries[ScrollToMiddleIndex > End ? End : Start]).Shrink(GrowShrinkInSameDirectionAsScrollToMiddle());   
                }
                else
                {
                    TestRecyclerEditorLogger.LogWarning($"Cannot test this behaviour while the middle entry {ScrollToMiddleIndex} is currently visible");
                }
            }

            /*** Immediate Scroll ***/
            // Scroll immediate to top/left edge of the middle index
            else if ((Input.GetKey(KeyCode.I) && Input.GetKeyDown(KeyCode.M)) || DemoToolbar.GetButtonDown(5))
            {
                _recycler.ScrollToIndexImmediate(ScrollToMiddleIndex, _recycler.Orientation.IsVertical() ? ScrollToAlignment.VerticalEntryTop : ScrollToAlignment.HorizontalEntryLeft);
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
                _recycler.ScrollToIndexImmediate(ScrollToMiddleIndex, ScrollToAlignment.VerticalEntryTop);
            }
            // Scroll immediate bottom edge
            else if ((Input.GetKey(KeyCode.E) && Input.GetKeyDown(KeyCode.B)) || DemoToolbar.GetButtonDown(9))
            {
                _recycler.ScrollToIndexImmediate(ScrollToMiddleIndex, ScrollToAlignment.VerticalEntryBottom);
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
        
        private FixEntries GrowShrinkInSameDirectionAsScrollToMiddle()
        {
            (int Start, int End) = _window.VisibleIndexRange.Value;

            switch (_recycler.Orientation)
            {
                case RecyclerScrollRectOrientation.TopToBottom:
                    return ScrollToMiddleIndex > End ? FixEntries.VerticalAbove : FixEntries.VerticalBelow;

                case RecyclerScrollRectOrientation.BottomToTop:
                    return ScrollToMiddleIndex > End ? FixEntries.VerticalBelow : FixEntries.VerticalAbove;

                case RecyclerScrollRectOrientation.LeftToRight:
                    return ScrollToMiddleIndex > End ? FixEntries.HorizontalLeft : FixEntries.HorizontalRight;

                case RecyclerScrollRectOrientation.RightToLeft:
                    return ScrollToMiddleIndex > End ? FixEntries.HorizontalRight : FixEntries.HorizontalLeft;
                
                default:
                    TestRecyclerEditorLogger.LogWarning($"Unknown {nameof(FixEntries)} value needed to grow/shrink in the same direction as scrolling to the middle index.");
                    return _recycler.Orientation.IsVertical() ? FixEntries.VerticalAbove : FixEntries.HorizontalLeft;
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
