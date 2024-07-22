using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Entry for testing clearing and adding entries to a recycler, one-by-one
    /// </summary>
    public class ClearAndFillEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Text _indexText = null;

        protected override void OnBindNewData(EmptyRecyclerData entryData)
        {
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

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
