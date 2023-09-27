using UnityEditor;
using UnityEditor.UI;

/// <summary>
/// Custom inspector for RecyclerScrollRects
/// </summary>
[CustomEditor(typeof(RecyclerScrollRect<>), true)]
public class RecyclerScrollRectEditor : ScrollRectEditor
{
    /// <summary>
    /// Draws our default inspector after Unity's custom ScrollRect inspector
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
