using System;

namespace Swill.Recycler
{
    /// <summary>
    /// Indicates that a method is intended to be called by a recycler entry or endcap for internal use, and not the user.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CalledByRecyclerContentAttribute : Attribute
    {
    }
}
