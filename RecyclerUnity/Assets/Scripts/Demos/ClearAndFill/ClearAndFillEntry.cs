using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Entry for testing clearing and adding entries to a recycler, one-by-one
/// </summary>
public class ClearAndFillEntry : RecyclerScrollRectEntry<EmptyRecyclerData, string>
{
    [SerializeField]
    private Text _indexText = null;
    
    protected override void OnBindNewData(EmptyRecyclerData entryData)
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