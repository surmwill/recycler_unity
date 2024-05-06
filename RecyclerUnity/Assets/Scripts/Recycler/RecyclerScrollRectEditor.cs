using UnityEditor;
using UnityEditor.UI;

/// <summary>
/// Override Unity's default ScrollRectEditor so we can see our custom fields too
/// </summary>
[CustomEditor(typeof(RecyclerScrollRect<>), true)]
public class RecyclerScrollRectEditor : ScrollRectEditor
{
    // Draw GUI
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
