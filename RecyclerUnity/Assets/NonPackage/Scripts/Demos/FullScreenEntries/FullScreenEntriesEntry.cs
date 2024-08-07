using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Entry for testing a recycler with full screen entries and endcap.
    /// </summary>
    public class FullScreenEntriesEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Text _indexText = null;

        private const float BufferPct = 0.1f;

        protected override void OnBindNewData(EmptyRecyclerData entryData)
        {
            _indexText.text = Index.ToString();
            
            // Add a bit of a buffer just to be safe
            RectTransform.sizeDelta = RectTransform.sizeDelta.WithY(Screen.height + Screen.height * BufferPct);
        }

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
