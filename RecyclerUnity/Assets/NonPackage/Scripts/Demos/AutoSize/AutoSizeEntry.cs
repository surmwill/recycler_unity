using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Recycler entry to test if we can handle auto-sized entries.
    /// </summary>
    public class AutoSizeEntry : RecyclerScrollRectEntry<AutoSizeData, string>
    {
        [SerializeField]
        private Text _titleText = null;

        [SerializeField]
        private Text _linesText = null;
        
        private const int AppendMinLinesOfText = 1;
        private const int AppendMaxLinesOfText = 4;

        protected override void OnBindNewData(AutoSizeData entryData)
        {
            _titleText.text = $"Randomly Generated <color=red>{entryData.NumLines}</color> Line(s)";
            UpdateLines();
        }

        /// <summary>
        /// Appends additional lines to the text
        /// </summary>
        public void AppendLines()
        {
            Data.NumAppendedLines += Random.Range(AppendMinLinesOfText, AppendMaxLinesOfText);
            UpdateLines();
            RecalculateHeight(null, FixEntries.Above);
        }

        private void UpdateLines()
        {
            _linesText.text = Enumerable.Range(0, Data.NumLines)
                .Aggregate(string.Empty, (s, i) => s + $"Line {i + 1}\n")
                .Trim();

            if (Data.NumAppendedLines > 0)
            {
                _linesText.text += "\n";
            }

            _linesText.text += Enumerable.Range(0, Data.NumAppendedLines)
                .Aggregate(string.Empty, (s, i) => s + $"Appended line {i + 1}\n")
                .Trim();

        }
    }
}
