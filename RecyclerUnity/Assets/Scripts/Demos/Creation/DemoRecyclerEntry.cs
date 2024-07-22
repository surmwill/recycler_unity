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

        // Called when this entry is bound to new data
        protected override void OnBindNewData(DemoRecyclerData entryData, RecyclerScrollRectContentState state)
        {
            // Set the word and background color to whatever is passed in the data
            _wordText.text = entryData.Word;
            _background.color = entryData.BackgroundColor;

            // Display the index (note that Index is a property found in the base class)
            _indexText.text = Index.ToString();
        }

        // Called when this entry is bound with data it had before (and therefore still currently has)
        protected override void OnRebindExistingData(RecyclerScrollRectContentState state)
        {
            // Debug.Log(Data.Word);
            // Debug.Log(Data.BackgroundColor);
        }

        // Called when this entry has been sent back to the recycling pool
        protected override void OnSentToRecycling()
        {

        }
        
        // Called when the active state of an entry changes, that is, when it moves from cached -> visible or visible -> cached
        protected override void OnActiveStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
        {
        }
    }
}
