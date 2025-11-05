using UnityEngine;
using UnityEngine.UI;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Recycler entry for demoing appending
    /// </summary>
    public class AppendPrependEntry : RecyclerScrollRectEntry<string, EmptyRecyclerData>
    {
        [SerializeField]
        private Text _indexText = null;

        protected override void OnBind(EmptyRecyclerData _)
        {
            // Empty
        }

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
