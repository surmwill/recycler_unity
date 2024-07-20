#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Helpful editor functions
    /// </summary>
    public static class EditorUtils
    {
        /// <summary>
        /// Destroy is only allowed during runtime and the alternative, DestroyImmediate, is not allowed in OnValidate.
        /// If we wish to destroy something during OnValidate we'll need to move the actual destruction outside of the call.
        /// </summary>
        public static void OnValidateDestroy(Object obj)
        {
            EditorApplication.delayCall += () => { Object.DestroyImmediate(obj); };
        }
    }
}

#endif
