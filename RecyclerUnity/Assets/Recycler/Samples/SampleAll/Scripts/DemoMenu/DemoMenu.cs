using System.Linq;
using UnityEngine;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Menu that allows us to move around to different demo scenes.
    /// </summary>
    public class DemoMenu : MonoBehaviour
    {
        [SerializeField]
        private DemoMenuRecycler _demoMenuRecycler = null;
        
        private void Start()
        {
            _demoMenuRecycler.AppendEntries(RecyclerDemoSceneNames.Names
                .Where(sceneName => sceneName != RecyclerDemoSceneNames.DemoMenuSceneName)
                .Select(sceneName => new DemoMenuData(sceneName)));
        }
    }
}
