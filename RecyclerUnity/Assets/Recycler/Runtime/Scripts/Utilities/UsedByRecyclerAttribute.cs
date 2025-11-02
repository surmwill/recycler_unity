using System;

namespace Swill.Recycler
{
    /// <summary>
    /// Indicates that the method or property is intended to be used by the recycler for internal use, and not the user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class UsedByRecyclerAttribute : Attribute
    {
    }
}
