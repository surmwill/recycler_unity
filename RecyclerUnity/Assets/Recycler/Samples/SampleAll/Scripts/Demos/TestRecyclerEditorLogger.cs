using UnityEngine;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Logs debug messages, but only in the editor
    /// </summary>
    public static class TestRecyclerEditorLogger
    {
        /// <summary>
        /// Logs a debug message, but only if we're in the editor
        /// </summary>
        /// <param name="message"> The debug message </param>
        public static void Log(string message)
        {
            #if UNITY_EDITOR
            Debug.Log(message);
            #endif
        }
        
        /// <summary>
        /// Logs a debug warning message, but only if we're in the editor
        /// </summary>
        /// <param name="message"> The debug message </param>
        public static void LogWarning(string message)
        {
            #if UNITY_EDITOR
            Debug.LogWarning(message);
            #endif
        }

        /// <summary>
        /// Logs a debug error message, but only if we're in the editor
        /// </summary>
        /// <param name="errorMessage"> The debug error message </param>
        public static void LogError(string errorMessage)
        {
            #if UNITY_EDITOR
            Debug.LogError(errorMessage);
            #endif
        }

        /// <summary>
        /// Logs a debug error message and breaks, but only if we're in the editor
        /// </summary>
        /// <param name="errorMessage"> The debug error message </param>
        public static void LogErrorAndBreak(string errorMessage)
        {
            #if UNITY_EDITOR
            Debug.LogError(errorMessage);
            Debug.Break();
            #endif
        }
    }   
}
