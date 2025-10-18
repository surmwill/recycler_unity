using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Recycler entry for demoing resizing the endcap
    /// </summary>
    public class EndcapResizeEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Text _numberText = null;

        protected override void OnBind(EmptyRecyclerData entryData)
        {
        }

        private void Update()
        {
            _numberText.text = Index.ToString();
        }
    }
}
