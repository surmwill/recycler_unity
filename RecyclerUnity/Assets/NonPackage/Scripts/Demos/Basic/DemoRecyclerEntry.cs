using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Example of a recycler entry for demoing purposes
    /// </summary>
    public class DemoRecyclerEntry : RecyclerScrollRectEntry<DemoRecyclerData, string>
    {
        [SerializeField]
        private Text _wordText = null;

        [SerializeField]
        private Text _indexText = null;

        [SerializeField]
        private Image _background = null;

        // Mandatory: Called when this entry is bound to new data
        protected override void OnBindNewData(DemoRecyclerData entryData)
        {
            // Set the word and background color to whatever is passed in the data
            _wordText.text = entryData.Word;
            _background.color = entryData.BackgroundColor;

            // Display the index (note that Index is a property found in the base class)
            _indexText.text = Index.ToString();
        }

        // Optional: called when this entry is bound with data it had before (and therefore still currently has)
        protected override void OnRebindExistingData()
        {
            // Debug.Log(Data.Word);
            // Debug.Log(Data.BackgroundColor);
        }

        // Optional: Called when this entry has been sent back to the recycling pool
        protected override void OnSentToRecycling()
        {

        }
        
        // Optional: called when the state of the entry changes
        protected override void OnStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
        {
        }
    }
}
