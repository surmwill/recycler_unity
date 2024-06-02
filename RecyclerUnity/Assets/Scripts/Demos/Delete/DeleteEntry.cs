using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeleteEntry : RecyclerScrollRectEntry<object>
{
    [SerializeField]
    private TMP_Text _indexText = null;
    
    protected override void OnBindNewData(object _)
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
