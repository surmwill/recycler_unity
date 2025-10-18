using UnityEngine;

namespace com.swill.recycler
{
    /// <summary>
    /// Tests changing entries' colours as they move between non-visible and visible
    /// </summary>
    public class TestStateChangesRecycler : TestRecycler<EmptyRecyclerData, string>
    {
        [SerializeField]
        private EmptyRecyclerScrollRect _recycler = null;

        private const int InitNumEntries = 50;
        
        // The time it takes for the entries and endcap to change colors
        public const float CrossFadeTimeSeconds = 1.5f;

        // Colors corresponding to the different states of the entries
        public static readonly Color OnVisibleColor = new(0xFB / 255f, 0xAF / 255f, 0x00 / 255f);
        public static readonly Color OnNotVisibleColor = new(0x00 / 255f, 0x7C / 255f, 0xBE / 255f);
        
        protected override RecyclerScrollRect<EmptyRecyclerData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "State change demo";

        protected override string DemoDescription => "Tests changing entries' colours as they move between non-visible and visible";

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
