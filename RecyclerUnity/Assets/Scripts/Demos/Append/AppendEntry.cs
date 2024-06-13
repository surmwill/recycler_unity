using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Recycler entry for demoing appending
/// </summary>
public class AppendEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
{
    [SerializeField]
    private TMP_Text _indexText = null;
    
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
