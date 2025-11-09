using UnityEngine;
using UnityEngine.UI;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Entry for testing a recycler with full screen entries and endcap.
    /// </summary>
    public class FullScreenEntriesEntry : RecyclerScrollRectEntry<string, EmptyRecyclerData>
    {
        [SerializeField]
        private Text _indexText = null;

        private const float Buffer = 1.1f;

        protected override void OnBind(EmptyRecyclerData entryData)
        {
            _indexText.text = Index.ToString();
            
            // Add a bit of a buffer just to be safe
            RectTransform.sizeDelta = Recycler.Orientation.IsVertical() ? RectTransform.sizeDelta.WithY(Screen.height * Buffer) : RectTransform.sizeDelta.WithX(Screen.width * Buffer);
        }

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
