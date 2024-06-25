using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrintNeedingRebuild : MonoBehaviour, ILayoutElement
{

    public void CalculateLayoutInputHorizontal()
    {
        Debug.Log("CALCULATING LAYOUT " + Time.frameCount);
    }

    public void CalculateLayoutInputVertical()
    {
    }

    public float minWidth { get; }
    public float preferredWidth { get; }
    public float flexibleWidth { get; }
    public float minHeight { get; }
    public float preferredHeight { get; }
    public float flexibleHeight { get; }
    public int layoutPriority { get; }
}
