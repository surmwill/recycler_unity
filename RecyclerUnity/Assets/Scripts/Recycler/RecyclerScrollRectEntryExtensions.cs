using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Extensions for scroll rect entries
/// </summary>
public static class RecyclerScrollRectEntryExtensions
{
    /// <summary>
    /// Detects when the scroll rect entry has been on screen for a certain period of time.
    /// Note tt follows that the entry is guaranteed to on-screen when the completion callback fires.
    /// </summary>
    /*
    public static IEnumerator DetectPersistenceOnScreen<T>(
        this RecyclerScrollRectEntry<T> entry, 
        float timeInSeconds, 
        Action onComplete)
    {
        float? _shownStartTime = null; 
        
        for (;;)
        {
            if (entry.IsVisible)
            {
                if (!_shownStartTime.HasValue)
                {
                    _shownStartTime = Time.realtimeSinceStartup;   
                }
                else if (Time.realtimeSinceStartup - _shownStartTime >= timeInSeconds)
                {
                    break;
                }
            }
            else
            {
                _shownStartTime = null;
            }

            yield return null;
        }
        
        onComplete.Invoke();
    }
    */
}
