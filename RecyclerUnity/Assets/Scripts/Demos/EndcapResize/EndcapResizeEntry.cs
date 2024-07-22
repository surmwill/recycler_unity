using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Recycler entry for demoing resizing the endcap
    /// </summary>
    public class EndcapResizeEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Text _numberText = null;

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
            _numberText.text = Index.ToString();
        }
    }
}
