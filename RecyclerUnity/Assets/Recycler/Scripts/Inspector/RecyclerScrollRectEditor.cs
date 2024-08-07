#if UNITY_EDITOR
using UnityEditor;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Override Unity's default ScrollRectEditor so we can see our custom fields too
    /// </summary>
    [CustomEditor(typeof(RecyclerScrollRect<,>), true)]
    public class RecyclerScrollRectEditor : ScrollRectWithDragSensitivityEditor
    {
        // General
        private SerializedProperty _recyclerEntryPrefab = null;
        private SerializedProperty _numCachedAtEachEnd = null;
        private SerializedProperty _appendTo = null;

        // Pool
        private SerializedProperty _poolParent = null;
        private SerializedProperty _poolSize = null;

        // Endcap (optional)
        private SerializedProperty _endcapPrefab = null;
        private SerializedProperty _endcapParent = null;
        private SerializedProperty _endcap = null;

        // Extra
        private SerializedProperty _setTargetFrameRateTo60 = null;

        protected override void OnEnable()
        {
            base.OnEnable();

            // General
            _recyclerEntryPrefab = serializedObject.FindProperty(nameof(_recyclerEntryPrefab));
            _numCachedAtEachEnd = serializedObject.FindProperty(nameof(_numCachedAtEachEnd));
            _appendTo = serializedObject.FindProperty(nameof(_appendTo));

            // Pool
            _poolParent = serializedObject.FindProperty(nameof(_poolParent));
            _poolSize = serializedObject.FindProperty(nameof(_poolSize));

            // Endcap
            _endcapPrefab = serializedObject.FindProperty(nameof(_endcapPrefab));
            _endcapParent = serializedObject.FindProperty(nameof(_endcapParent));
            _endcap = serializedObject.FindProperty(nameof(_endcap));

            // Extra
            _setTargetFrameRateTo60 = serializedObject.FindProperty(nameof(_setTargetFrameRateTo60));
        }

        // Draw GUI
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            // General
            EditorGUILayout.PropertyField(_recyclerEntryPrefab);
            EditorGUILayout.PropertyField(_numCachedAtEachEnd);
            EditorGUILayout.PropertyField(_appendTo);

            // Pool
            EditorGUILayout.PropertyField(_poolParent);
            EditorGUILayout.PropertyField(_poolSize);

            // Endcap
            EditorGUILayout.PropertyField(_endcapPrefab);
            EditorGUILayout.PropertyField(_endcapParent);
            EditorGUILayout.PropertyField(_endcap);

            // Extra
            EditorGUILayout.PropertyField(_setTargetFrameRateTo60);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

#endif