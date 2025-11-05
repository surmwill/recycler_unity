
using UnityEngine;

namespace Swill.Recycler
{
    /// <summary>
    /// Extensions for the FixEntries enum
    /// </summary>
    public static class FixEntriesExtensions
    {
        private static FixEntries? _validFixEntries;
        
        /// <summary>
        /// The user should be using the appropriate value of FixEntries depending on the orientation of the list.
        /// For example, fixing below entries doesn't make sense in a horizontal list. If they provide an invalid value
        /// then a fallback valid one will be substituted and warning relayed.
        /// </summary>
        /// <param name="candidateFixEntries"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        public static FixEntries ValidateWithOrientation(this FixEntries candidateFixEntries, RecyclerScrollRectOrientation orientation)
        {
            _validFixEntries = null;
            
            if (orientation.IsHorizontal())
            {
                if (candidateFixEntries == FixEntries.Above)
                {
                    _validFixEntries = FixEntries.Left;
                }
                
                if (candidateFixEntries == FixEntries.Below)
                {
                    _validFixEntries = FixEntries.Right;
                }
            }
            else
            {
                if (candidateFixEntries == FixEntries.Left)
                {
                    _validFixEntries = FixEntries.Above;
                }

                if (candidateFixEntries == FixEntries.Right)
                {
                    _validFixEntries = FixEntries.Below;
                }
            }

            if (_validFixEntries.HasValue)
            {
                Debug.LogWarning($"Recycler: a FixEntries value `{candidateFixEntries}` does not align with the current recycler `{(orientation.IsVertical() ? "vertical" : "horizontal")}` orientation. " +
                                 $"Falling back to a FixEntries value of `{_validFixEntries.Value}`.");
            }
            else
            {
                _validFixEntries = candidateFixEntries;
            }

            return _validFixEntries.Value;
        }
    }
}
