using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests a recycler working with auto-sized entries
    /// </summary>
    public class TestAutoSizeRecycler : MonoBehaviour
    {
        [SerializeField]
        private AutoSizeRecycler _autoSizeRecycler = null;

        private const int NumEntries = 30;

        private const int MinNumLines = 1;
        private const int MaxNumLines = 6;

        private void Start()
        {
            _autoSizeRecycler.AppendEntries(Enumerable.Range(0, NumEntries)
                .Select(_ => new AutoSizeData(Random.Range(MinNumLines, MaxNumLines + 1))));
        }
    }
}
