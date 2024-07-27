using System;
using System.Reflection;
using RecyclerScrollRect;
using UnityEngine;

/// <summary>
/// Helpful reflection functions
/// </summary>
public static class RecyclerScrollRectReflectionHelpers
{
    /// <summary>
    /// Returns the value of a private field in a RecyclerScrollRect
    /// </summary>
    public static TFieldValue GetPrivateFieldValue<TFieldValue, TEntryData, TKeyEntryData>(RecyclerScrollRect<TEntryData, TKeyEntryData> recycler, string fieldName) 
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        FieldInfo field = typeof(RecyclerScrollRect<TEntryData, TKeyEntryData>).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        
        if (field == null)
        {
            throw new ArgumentException($"Field '{fieldName}' not found");
        }

        return (TFieldValue) field.GetValue(recycler);
    }
}
