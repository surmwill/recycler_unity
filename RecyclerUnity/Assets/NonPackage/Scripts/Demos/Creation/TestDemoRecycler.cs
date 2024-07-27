using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RecyclerScrollRect
{
    /// <summary>
    /// A simple demo recycler with entries and an endcap
    /// </summary>
    public class TestDemoRecycler : MonoBehaviour
    {
        [SerializeField]
        private DemoRecycler _recycler = null;
        
        private RecyclerValidityChecker<DemoRecyclerData, string> _validityChecker;

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

        private void Start()
        {
            _validityChecker = new RecyclerValidityChecker<DemoRecyclerData, string>(_recycler);
            _validityChecker.Bind();
            
            // Create data containing the words from the array, each with a random background color
            DemoRecyclerData[] entryData = new DemoRecyclerData[Words.Length];
            for (int i = 0; i < Words.Length; i++)
            {
                entryData[i] = new DemoRecyclerData(Words[i], Random.ColorHSV());
            }

            _recycler.AppendEntries(entryData);
        }

        private void OnDestroy()
        {
            _validityChecker.Unbind();
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
