using System;
using UnityEngine;

namespace com.swill.recycler
{
    /// <summary>
    /// Attribute to make a serialized field read-only in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ReadOnlyAttribute : PropertyAttribute
    {

    }
}