using UnityEngine;
using Random = UnityEngine.Random;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests a basic recycler with entries and an endcap - no special features.
    /// Used as an example in the documentation for how to create a basic recycler.
    /// </summary>
    public class TestDemoRecycler : TestRecycler<DemoRecyclerData, string>
    {
        [SerializeField]
        private DemoRecycler _recycler = null;

        private static readonly string[] Words =
        {
            "hold", "work", "wore", "days", "meat",
            "hill", "club", "boom", "tone", "grey",
            "bowl", "bell", "kick", "hope", "over",
            "year", "camp", "tell", "main", "lose",
            "earn", "name", "hang", "bear", "heat",
            "trip", "calm", "pace", "home", "bank",
            "cell", "lake", "fall", "fear", "mood",
            "head", "male", "evil", "toll", "base"
        };

        protected override RecyclerScrollRect<DemoRecyclerData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Simple recycler demo";

        protected override string DemoDescription => "Tests a basic recycler with no special functionality.\n" +
                                                     "Used as a simple example in the documentation.";

        protected override string[] DemoButtonDescriptions => null;

        protected override void Start()
        {
            base.Start();
            
            // Create data containing the words from the array, each with a random background color
            DemoRecyclerData[] entryData = new DemoRecyclerData[Words.Length];
            for (int i = 0; i < Words.Length; i++)
            {
                entryData[i] = new DemoRecyclerData(Words[i], Random.ColorHSV());
            }

            _recycler.AppendEntries(entryData);
        }

        private void OnValidate()
        {
            if (_recycler == null)
            {
                _recycler = GetComponent<DemoRecycler>();
            }
        }
    }
}
