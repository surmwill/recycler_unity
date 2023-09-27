using System;
using TMPro;
using UnityEngine;

/// <summary>
/// A recycler entry displaying a string (used for testing the recycler with simple data)
/// </summary>
public class StringScrollRectEntry : RecyclerScrollRectEntry<string>
{
    [SerializeField]
    private TMP_Text _text = null;
    
    protected override void OnBindNewData(string entryData)
    {
        _text.text = entryData;
    }

    protected override void OnRebindExistingData()
    {
        // Do nothing
    }
    
    protected override void OnSentToRecycling()
    {
        // Do nothing
    }

    protected void Update()
    {
        if (gameObject.name != "0")
        {
            return;
        }
        
        if (Input.GetKeyDown(KeyCode.W))
        {
            _text.text += _text.text;
            RecalculateDimensions();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            _text.text = _text.text.Substring(0, _text.text.Length / 2);
            RecalculateDimensions();
        }
    }
}