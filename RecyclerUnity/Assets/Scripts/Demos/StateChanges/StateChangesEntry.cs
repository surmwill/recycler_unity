using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Entry for testing clearing and adding entries to a recycler, one-by-one
    /// </summary>
    public class StateChangesEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Text _indexText = null;

        protected override void OnBindNewData(EmptyRecyclerData entryData)
        {
            _indexText.text = Index.ToString();
        }
    }
}
