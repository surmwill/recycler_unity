using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Recycler entry to test if we can handle auto-sized content
    /// </summary>
    public class AutoSizeEntry : RecyclerScrollRectEntry<AutoSizeData, string>
    {
        [SerializeField]
        private Text _titleText = null;

        [SerializeField]
        private Text _linesText = null;

        protected override void OnBindNewData(AutoSizeData entryData)
        {
            _titleText.text = $"Randomly Generated <color=red>{entryData.NumLines}</color> Line(s)";

            _linesText.text = Enumerable.Range(0, entryData.NumLines)
                .Aggregate(string.Empty, (s, i) => s + $"Line {i + 1}\n")
                .Trim();
        }

        protected override void OnRebindExistingData()
        {
        }

        protected override void OnSentToRecycling()
        {
        }

        protected override void OnActiveStateChanged(RecyclerScrollRectContentState? prevState, RecyclerScrollRectContentState newState)
        {
        }
    }
}
