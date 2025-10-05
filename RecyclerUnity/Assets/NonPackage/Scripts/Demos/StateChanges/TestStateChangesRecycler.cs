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

        private const int InitNumEntries = 50;

        // Colors corresponding to the different states of the entries
        public static readonly Color OnVisibleColor = new(0xFB / 255f, 0xAF / 255f, 0x00 / 255f);
        public static readonly Color OnStartCacheColor = new(0x00 / 255f, 0x7C / 255f, 0xBE / 255f);
        public static readonly Color OnEndCacheColor = new(0x00 / 255f, 0xAF / 255f, 0x54 / 255f);
        
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
