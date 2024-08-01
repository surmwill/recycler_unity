

namespace RecyclerScrollRect
{
    /// <summary>
    /// Data representing the name of a demo scene to navigate to.
    /// </summary>
    public class DemoMenuData : IRecyclerScrollRectData<string>
    {
        /// <summary>
        /// The unique key 
        /// </summary>
        public string Key => SceneName;
        
        public string SceneName { get; }

        public DemoMenuData(string sceneName)
        {
            SceneName = sceneName;
        }
    }
}
