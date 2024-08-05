using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests changing entries' colours as they move states from cached to visible.
    /// </summary>
    public class TestStateChangesRecycler : TestRecycler<EmptyRecyclerData, string>
    {
        [SerializeField]
        private EmptyRecyclerScrollRect _recycler = null;
        
        // The time it takes for the entries and endcap to change colors
        public const float CrossFadeTimeSeconds = 1.5f;
        
        // Output state changes for the entry with this index
        public const int DebugPrintStateChangesForEntryIndex = 15;

        private const int InitNumEntries = 50;

        protected override RecyclerScrollRect<EmptyRecyclerData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "State change demo";

        protected override string DemoDescription =>
            "Tests changing an entries' colours as they move from the start cache, to visible, to the end cache";

        protected override string[] DemoButtonDescriptions => null;

        protected override void Start()
        {
            base.Start();
            _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitNumEntries));
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
