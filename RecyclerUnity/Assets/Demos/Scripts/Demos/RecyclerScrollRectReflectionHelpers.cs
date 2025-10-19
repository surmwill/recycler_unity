using System;
using System.Reflection;

namespace Swill.Recycler.Demos
{
    /// <summary>
    /// Helpful reflection functions
    /// </summary>
    public static class RecyclerScrollRectReflectionHelpers
    {
        /// <summary>
        /// Returns the value of a private field in a RecyclerScrollRect
        /// </summary>
        public static TFieldValue GetPrivateFieldValue<TFieldValue, TEntryData, TKeyEntryData>(RecyclerScrollRect<TKeyEntryData, TEntryData> recycler, string fieldName) where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
        {
            FieldInfo field = typeof(RecyclerScrollRect<TKeyEntryData, TEntryData>).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new ArgumentException($"Field '{fieldName}' not found");
            }

            return (TFieldValue) field.GetValue(recycler);
        }
    }
}
