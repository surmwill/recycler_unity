using System;

namespace Swill.Recycler
{
    /// <summary>
    /// Indicates that a method is intended to be used by a recycler entry/endcap for internal use, and not the user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class UsedByRecyclerContentAttribute : Attribute
    {
    }
}
