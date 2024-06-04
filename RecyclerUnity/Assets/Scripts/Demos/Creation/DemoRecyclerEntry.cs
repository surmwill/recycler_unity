using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DemoRecyclerEntry : RecyclerScrollRectEntry<DemoRecyclerData>
{
    [SerializeField]
    private TMP_Text _wordText = null;

    [SerializeField]
    private TMP_Text _indexText = null;

    [SerializeField]
    private Image _background = null;
    
    // Called when this entry is bound to new data
    protected override void OnBindNewData(DemoRecyclerData entryData)
    {
        _wordText.text = entryData.Word;
        _background.color = entryData.BackgroundColor;
        
        _indexText.text = Index.ToString();
    }

    // Called when this entry is bound with data it had before (and therefore still currently has)
    protected override void OnRebindExistingData()
    {
        Debug.Log(Data.Word);
        Debug.Log(Data.BackgroundColor);
    }

    // Called when this entry has been sent back to the recycling pool
    protected override void OnSentToRecycling()
    {
       
    }
}
