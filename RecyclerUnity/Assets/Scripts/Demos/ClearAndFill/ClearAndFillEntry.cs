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

        protected override void OnBindNewData(EmptyRecyclerData entryData, RecyclerScrollRectContentState state)
        {
        }

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
