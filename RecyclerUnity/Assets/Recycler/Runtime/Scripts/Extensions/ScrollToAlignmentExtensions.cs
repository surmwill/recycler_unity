using UnityEngine;

namespace Swill.Recycler
{
    /// <summary>
    /// Extensions for the ScrollToAlignment enum
    /// </summary>
    public static class ScrollToAlignmentExtensions
    {
         private static ScrollToAlignment? _validScrollToAlignment;
        
        /// <summary>
        /// The user should be using the appropriate values of FixEntries depending on the orientation of the list.
        /// For example, scrolling to the top of an entry while in a horizontal recycler doesn't make any sense.
        /// </summary>
        /// <param name="candidateScrollToAlignment"> The ScrollToAlignment value the user is trying to use </param>
        /// <param name="orientation"> The orientation of the recycler </param>
        /// <returns> A valid value of ScrollToAlignment for the Recycler </returns>
        public static ScrollToAlignment ValidateWithOrientation(this ScrollToAlignment candidateScrollToAlignment, RecyclerScrollRectOrientation orientation)
        {
            _validScrollToAlignment = null;
            
            if (orientation.IsHorizontal())
            {
                if (candidateScrollToAlignment == ScrollToAlignment.VerticalEntryTop)
                {
                    _validScrollToAlignment = ScrollToAlignment.HorizontalEntryLeft;
                }
                
                if (candidateScrollToAlignment == ScrollToAlignment.VerticalEntryBottom)
                {
                    _validScrollToAlignment = ScrollToAlignment.HorizontalEntryRight;
                }
            }
            else
            {
                if (candidateScrollToAlignment == ScrollToAlignment.HorizontalEntryLeft)
                {
                    _validScrollToAlignment = ScrollToAlignment.VerticalEntryTop;
                }

                if (candidateScrollToAlignment == ScrollToAlignment.HorizontalEntryRight)
                {
                    _validScrollToAlignment = ScrollToAlignment.VerticalEntryBottom;
                }
            }

            if (_validScrollToAlignment.HasValue)
            {
                Debug.LogWarning($"Recycler: a {nameof(ScrollToAlignment)} value `{candidateScrollToAlignment}` does not align with the current recycler `{(orientation.IsVertical() ? "vertical" : "horizontal")}` orientation. " +
                                 $"Falling back to a {nameof(ScrollToAlignment)} value of `{_validScrollToAlignment.Value}`.");
            }
            else
            {
                _validScrollToAlignment = candidateScrollToAlignment;
            }

            return _validScrollToAlignment.Value;
        }
    }
}