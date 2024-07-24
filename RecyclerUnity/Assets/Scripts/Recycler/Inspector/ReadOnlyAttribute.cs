using System;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Attribute to make a serialized field read-only in the inspector
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute
    {

    }
}