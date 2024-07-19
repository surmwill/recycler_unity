#if UNITY_EDITOR
using System;
using UnityEngine;

/// <summary>
/// Attribute to make a serialized field read-only in the inspector
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class ReadOnlyAttribute : PropertyAttribute
{
    
}
#endif
