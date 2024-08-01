using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Recycler entry for navigating between different demo scenes.
    /// </summary>
    public class DemoMenuRecyclerEntry : RecyclerScrollRectEntry<DemoMenuData, string>
    {
        [SerializeField]
        private Text _demoName = null;
        
        protected override void OnBindNewData(DemoMenuData entryData)
        {
            _demoName = entryData.Key;
        }
    }
}
