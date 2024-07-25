using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests that our recycler works with a Screen Space - Camera Canvas
    /// </summary>
    public class CanvasCameraEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Text _indexText = null;

        protected override void OnBindNewData(EmptyRecyclerData entryData)
        {
        }

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
