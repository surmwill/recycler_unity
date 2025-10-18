using UnityEngine;
using UnityEngine.UI;

namespace Swill.Recycler
{
    /// <summary>
    /// Entry for testing clearing and adding entries to a recycler, one-by-one
    /// </summary>
    public class ClearAndFillEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Text _indexText = null;

        protected override void OnBind(EmptyRecyclerData entryData)
        {
            _indexText.text = Index.ToString();
        }

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
