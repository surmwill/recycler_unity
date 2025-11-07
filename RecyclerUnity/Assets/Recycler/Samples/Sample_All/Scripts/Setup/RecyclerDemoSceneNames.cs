using System.Linq;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// The names of the demo scenes
    /// </summary>
    public static class RecyclerDemoSceneNames
    {
        private const string Prefix = "RecyclerDemo_";

        /// <summary>
        /// The main menu is the first listed demo scene
        /// </summary>
        public static string DemoMenuSceneName => Names[0];
        
        /// <summary>
        /// The names of the demo scenes
        /// </summary>
        public static readonly string[] Names = new [] 
        {
            "DemoMenu",
            "AppendPrepend",
            "AutoSize_Vertical",
            "AutoSize_Horizontal",
            "Basic",
            "CanvasCamera",
            "ClearAndFill",
            "Delete",
            "EndcapResize",
            "FullScreenEntries",
            "InsertAndResize",
            "PrettyInsertDelete",
            "ScrollToIndex",
            "StateChanges",
        }.Select(name => $"{Prefix}{name}").ToArray();
    }
}
