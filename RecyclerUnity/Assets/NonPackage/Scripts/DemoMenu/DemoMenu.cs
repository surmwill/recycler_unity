using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Menu that allows us to move around to different demo scenes.
    /// </summary>
    public class DemoMenu : MonoBehaviour
    {
        [SerializeField]
        private DemoMenuRecycler _demoMenuRecycler = null;

        [SerializeField]
        private string[] _sceneNames = null;
        
        private void Start()
        {
            _demoMenuRecycler.AppendEntries(_sceneNames.Select(sceneName => new DemoMenuData(sceneName)));
        }

        private void OnValidate()
        {
            #if UNITY_EDITOR
            _sceneNames = EditorBuildSettings.scenes
                .Select(scenePath => Path.GetFileNameWithoutExtension(scenePath.path))
                .Where(sceneName => sceneName != SceneManager.GetActiveScene().name)
                .ToArray();
            #endif
        }
    }
}
