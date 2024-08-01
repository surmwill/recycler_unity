using UnityEngine;
using UnityEngine.SceneManagement;
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

        [SerializeField]
        private Button _loadSceneButton = null;
        
        protected override void OnBindNewData(DemoMenuData entryData)
        {
            _demoName.text = entryData.SceneName;
            _loadSceneButton.onClick.AddListener(LoadScene);
        }

        protected override void OnSentToRecycling()
        {
            _loadSceneButton.onClick.RemoveListener(LoadScene);
        }

        private void LoadScene()
        {
            SceneManager.LoadScene(Data.SceneName, LoadSceneMode.Single);
        }
    }
}
