using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Swill.Recycler.Demos
{
    [CustomEditor(typeof(SetupRecyclerDemos))]
    public class SetupRecyclerDemosEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Setup"))
            {
                Debug.Log("Adding Missing Recycler Demo Scenes to Build Settings");
                AddMissingDemoScenes();
            }
        }

        private void AddMissingDemoScenes()
        {
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
                
            HashSet<string> buildSceneNames = new HashSet<string>(buildScenes.Select(scene => Path.GetFileNameWithoutExtension(scene.path)));
            HashSet<string> demoSceneNames = new HashSet<string>(RecyclerDemoSceneNames.Names);
                
            demoSceneNames.RemoveWhere(demoSceneName => buildSceneNames.Contains(demoSceneName));

            List<EditorBuildSettingsScene> addedDemoScenes = new List<EditorBuildSettingsScene>();
            foreach (string demoSceneNameToAdd in demoSceneNames)
            {
                string guid = AssetDatabase.FindAssets($"t:Scene {demoSceneNameToAdd}").FirstOrDefault();
                if (guid == null)
                {
                    Debug.LogWarning($"Could not find the recycler demo scene: {demoSceneNameToAdd}");
                    continue;
                }

                Debug.Log($"Added recycler demo scene: {demoSceneNameToAdd}");
                addedDemoScenes.Add(new EditorBuildSettingsScene(AssetDatabase.GUIDToAssetPath(guid), true));
            }

            if (addedDemoScenes.Any())
            {
                EditorBuildSettings.scenes = buildScenes.Concat(addedDemoScenes).ToArray();
            }
            else
            {
                Debug.Log("Nothing to add");
            }
        }
    }   
}
