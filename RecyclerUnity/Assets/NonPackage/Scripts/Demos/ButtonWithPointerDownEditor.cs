#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Editor for a button with an additional OnPointerDown callback.
    /// </summary>
    [CustomEditor(typeof(ButtonWithPointerDown))]
    public class ButtonWithPointerDownEditor : ButtonEditor
    {
        private SerializedProperty _onPointerDownEvent;

        protected override void OnEnable()
        {
            base.OnEnable();
            _onPointerDownEvent = serializedObject.FindProperty(nameof(_onPointerDownEvent));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(_onPointerDownEvent);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
