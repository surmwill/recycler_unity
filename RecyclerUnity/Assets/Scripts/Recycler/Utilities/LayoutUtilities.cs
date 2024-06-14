using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Utilities for dealing with layouts
/// </summary>
public static class LayoutUtilities
{
    /// <summary>
    /// Returns all behaviours that contribute to layout calculations
    /// </summary>
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
