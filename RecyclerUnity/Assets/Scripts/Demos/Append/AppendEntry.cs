using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Recycler entry for demoing appending
    /// </summary>
    public class AppendEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
    {
        [SerializeField]
        private Text _indexText = null;

        protected override void OnBindNewData(EmptyRecyclerData _, RecyclerScrollRectContentState state)
        {
        }

        private void Update()
        {
            _indexText.text = Index.ToString();
        }
    }
}
