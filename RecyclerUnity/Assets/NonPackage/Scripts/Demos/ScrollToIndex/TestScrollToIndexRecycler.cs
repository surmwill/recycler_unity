using System;
using System.Collections;
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

        private const int InitNumEntries = 50;
        private const int ScrollToMiddleIndex = 25;
        private const float ScrollWhileGrowShrinkingSpeed = 0.5f;

        private static readonly int[] EnlargeEntryIndices = { 41, 42 };

        protected override RecyclerScrollRect<ScrollToIndexData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Scroll to index demo";

        protected override string DemoDescription => "Tests scrolling behaviour.";
        
        protected override string[] DemoButtonDescriptions { get; }

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
            if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.M))
            {
                _recycler.ScrollToIndex(ScrollToMiddleIndex, onScrollComplete:() => Debug.Log("Middle index scroll complete."));
            }
            // Scroll to top index
            else if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.T))
            {
                _recycler.ScrollToIndex(0, onScrollComplete:() => Debug.Log("Top index scroll complete."));
            }
            // Scroll to bot index
            else if (Input.GetKey(KeyCode.A) && Input.GetKeyDown(KeyCode.B))
            {
                _recycler.ScrollToIndex(_recycler.DataForEntries.Count - 1, onScrollComplete:() => Debug.Log("Bottom index scroll complete."));
            }

            /*** Fighting ***/
            // Scroll to the middle while making the bottom visible entry grow, scrolling over the expanding entry
            else if (Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.G))
            {
                _recycler.ScrollToIndex(ScrollToMiddleIndex, scrollSpeedViewportsPerSecond:ScrollWhileGrowShrinkingSpeed);
                ((ScrollToIndexRecyclerScrollRectEntry) _recycler.ActiveEntries[_window.VisibleIndexRange.Value.End]).Grow(FixEntries.Above);
            }
            // Scroll to the middle while making the bottom visible entry shrink, scrolling over the shrinking entry
            else if (Input.GetKey(KeyCode.F) && Input.GetKeyDown(KeyCode.S))
            {
                _recycler.ScrollToIndex(ScrollToMiddleIndex, scrollSpeedViewportsPerSecond:ScrollWhileGrowShrinkingSpeed);
                ((ScrollToIndexRecyclerScrollRectEntry) _recycler.ActiveEntries[_window.VisibleIndexRange.Value.End]).Shrink(FixEntries.Above);
            }

            /*** Edges ***/
            // Scroll immediate top edge
            else if (Input.GetKey(KeyCode.E) && Input.GetKeyDown(KeyCode.T))
            {
                _recycler.ScrollToIndexImmediate(ScrollToMiddleIndex, ScrollToAlignment.EntryTop);
            }
            // Scroll immediate bottom edge
            else if (Input.GetKey(KeyCode.E) && Input.GetKeyDown(KeyCode.B))
            {
                _recycler.ScrollToIndexImmediate(ScrollToMiddleIndex, ScrollToAlignment.EntryBottom);
            }
            
            /*** Immediate Scroll ***/
            // Scroll immediate to middle index
            else if (Input.GetKey(KeyCode.I) && Input.GetKeyDown(KeyCode.M))
            {
                _recycler.ScrollToIndexImmediate(ScrollToMiddleIndex);
            }
            // Scroll immediate to top index
            else if (Input.GetKey(KeyCode.I) && Input.GetKeyDown(KeyCode.T))
            {
                _recycler.ScrollToIndexImmediate(0);
            }
            // Scroll immediate to bot index
            else if (Input.GetKey(KeyCode.I) && Input.GetKeyDown(KeyCode.B))
            {
                _recycler.ScrollToIndexImmediate(_recycler.DataForEntries.Count - 1);
            }
            
            /*** Other ***/
            // Test cancel scroll to
            else if (Input.GetKeyDown(KeyCode.C))
            {
                _recycler.CancelScrollTo();
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
