using System;

namespace Swill.Recycler
{
    /// <summary>
    /// Indicates that a method is intended to be called by the recycler for internal use, and not the user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CalledByRecyclerAttribute : Attribute
    {
    }
}
