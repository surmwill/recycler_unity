using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple recycler entry to test clearing and adding back entries one-by-one 
/// </summary>
public class ClearAndFillEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
{
    [SerializeField]
    private Text _indexText = null;

    protected override void OnBindNewData(EmptyRecyclerData _)
    {
    }

    protected override void OnRebindExistingData()
    {
    }

    protected override void OnSentToRecycling()
    {
    }
    
    private void Update()
    {
        _indexText.text = Index.ToString();
    }
}
