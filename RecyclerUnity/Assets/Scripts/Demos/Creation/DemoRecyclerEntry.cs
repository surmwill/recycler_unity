using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreationRecyclerScrollRectEntry : RecyclerScrollRectEntry<DemoRecyclerData>
{
    [SerializeField]
    private TMP_Text _word = null;

    [SerializeField]
    private TMP_Text _index = null;

    [SerializeField]
    private Image _background = null;
    
    protected override void OnBindNewData(DemoRecyclerData entryData)
    {
        
    }

    protected override void OnRebindExistingData()
    {

    }

    protected override void OnSentToRecycling()
    {
       
    }
}
