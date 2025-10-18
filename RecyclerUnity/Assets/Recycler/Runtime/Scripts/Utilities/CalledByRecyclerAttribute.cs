using System;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Indicates that a method is intended to be called by the recycler as part of the lifecycle management of the object, and not the user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CalledByRecyclerAttribute : Attribute
    {
    }
}
