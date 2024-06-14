#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Helpful editor functions
/// </summary>
public static class EditorUtils
{
    /// <summary>
    /// Destroy is only allowed during runtime and the alternative, DestroyImmediate, is not allowed in OnValidate.
    /// This moves the call outside of OnValidate
    /// </summary>
    public static void DestroyOnValidate(Object obj)
    {
        EditorApplication.delayCall += () =>
        {
            Object.DestroyImmediate(obj);
        };
    }
}

#endif
