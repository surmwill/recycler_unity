using System.Collections;
using System.Collections.Generic;
using RecyclerScrollRect;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Data representing the name of a demo scene to navigate to.
    /// </summary>
    public class DemoMenuData : MonoBehaviour, IRecyclerScrollRectData<string>
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
