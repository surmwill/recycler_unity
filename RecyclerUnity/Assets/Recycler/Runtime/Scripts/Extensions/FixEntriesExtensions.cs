
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
        /// The user should be using the appropriate values of FixEntries depending on the orientation of the list.
        /// For example, fixing below entries doesn't make sense in a horizontal list. If they provide an invalid value
        /// then a fallback valid one will be substituted and warning relayed.
        /// </summary>
        /// <param name="candidateFixEntries"> The FixEntries value the user is trying to use </param>
        /// <param name="orientation"> The orientation of the recycler </param>
        /// <returns> A valid value of FixEntries for the Recycler </returns>
        public static FixEntries ValidateWithOrientation(this FixEntries candidateFixEntries, RecyclerScrollRectOrientation orientation)
        {
            _validFixEntries = null;
            
            if (orientation.IsHorizontal())
            {
                if (candidateFixEntries == FixEntries.VerticalAbove)
                {
                    _validFixEntries = FixEntries.HorizontalLeft;
                }
                
                if (candidateFixEntries == FixEntries.VerticalBelow)
                {
                    _validFixEntries = FixEntries.HorizontalRight;
                }
            }
            else
            {
                if (candidateFixEntries == FixEntries.HorizontalLeft)
                {
                    _validFixEntries = FixEntries.VerticalAbove;
                }

                if (candidateFixEntries == FixEntries.HorizontalRight)
                {
                    _validFixEntries = FixEntries.VerticalBelow;
                }
            }

            if (_validFixEntries.HasValue)
            {
                Debug.LogWarning($"Recycler: a {nameof(FixEntries)} value `{candidateFixEntries}` does not align with the current recycler `{(orientation.IsVertical() ? "vertical" : "horizontal")}` orientation. " +
                                 $"Falling back to a {nameof(FixEntries)} value of `{_validFixEntries.Value}`.");
            }
            else
            {
                _validFixEntries = candidateFixEntries;
            }

            return _validFixEntries.Value;
        }
    }
}
