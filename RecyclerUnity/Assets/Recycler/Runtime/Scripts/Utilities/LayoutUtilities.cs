using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Utilities for dealing with layouts.
    /// </summary>
    public static class LayoutUtilities
    {
        /// <summary>
        /// Returns all Behaviours that contribute to costly layout calculations.
        /// </summary>
        /// <param name="g"> The GameObject to get the behaviours on. </param>
        /// <param name="includeDisabled"> Whether to include behaviours that are also disabled. </param>
        /// <returns> An array of Behaviours that contribute to costly layout calculations. </returns>
        public static Behaviour[] GetLayoutBehaviours(GameObject g, bool includeDisabled = false)
        {
            List<ILayoutElement> layoutElements = new List<ILayoutElement>();
            g.GetComponents(layoutElements);

            List<ILayoutController> layoutControllers = new List<ILayoutController>();
            g.GetComponents(layoutControllers);

            IEnumerable<Behaviour> layoutBehaviours = layoutElements.OfType<Behaviour>().Concat(layoutControllers.OfType<Behaviour>());
            return (includeDisabled ? layoutBehaviours : layoutBehaviours.Where(b => b.isActiveAndEnabled)).ToArray();
        }
    }
}
