using System.Linq;

namespace Swill.Recycler.Demos
{
    public static class RecyclerDemoSceneNames
    {
        private const string Prefix = "RecyclerDemo_";

        public static string DemoMenuSceneName => Names[0];
        
        public static readonly string[] Names = new [] 
        {
            "DemoMenu",
            "AppendPrepend",
            "AutoSize",
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
