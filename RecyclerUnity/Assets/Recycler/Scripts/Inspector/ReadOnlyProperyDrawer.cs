#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Draws a readonly property in the inspector
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyProperyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
}

#endif
