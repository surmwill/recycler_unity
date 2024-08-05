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
        private const int ScrollToIndex = 45;

        private static readonly int[] EnlargeEntryIndices = { 41, 42 };

        protected override RecyclerScrollRect<ScrollToIndexData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Scroll to index demo";

        protected override string DemoDescription => "Tests scrolling behaviour.";
        
        protected override string[] DemoButtonDescriptions { get; }

        protected override void Start()
        {
            base.Start();
            _recycler.AppendEntries(CreateEntryData(InitNumEntries, EnlargeEntryIndices));
        }

        private void Update()
        {
            // Scroll to middle index
            
            // Scroll to top index
            
            // Scroll to bottom index
            
            // Scroll to top slowly, and make entry above expand as rapidly as we scroll
            
            // Scroll to top slowly, and make entry above expand immediately, then decrease in size slowly 
            
            // Scroll immediate
            
            // Test scrolling to index
            
            // Test cancel scroll to
            if (Input.GetKeyDown(KeyCode.A))
            {
                _recycler.ScrollToImmediate(41);
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
