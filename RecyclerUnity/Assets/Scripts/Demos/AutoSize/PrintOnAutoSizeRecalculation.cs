using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Prints when a GameObject has its size recalculated to check for spam recalculations
/// </summary>
public class PrintOnAutoSizeRecalculation : MonoBehaviour, ILayoutElement
{
    public float minWidth { get; }
    
    public float preferredWidth { get; }
    
    public float flexibleWidth { get; }
    
    public float minHeight { get; }
    
    public float preferredHeight { get; }
    
    public float flexibleHeight { get; }
    
    public int layoutPriority { get; }
    
    public void CalculateLayoutInputHorizontal()
    {
        Debug.Log($"CALCULATING LAYOUT FOR {gameObject.name} AT {Time.frameCount}");
    }

    public void CalculateLayoutInputVertical()
    {
    }
    
    private void OnDisable()
    {
        enabled = true;
    }
}
