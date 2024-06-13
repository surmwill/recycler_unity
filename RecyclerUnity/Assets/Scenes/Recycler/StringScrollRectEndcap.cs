using TMPro;
using UnityEngine;

/// <summary>
/// A sample end-cap for testing
/// </summary>
public class StringScrollRectEndcap : RecyclerScrollRectEndcap<StringRecyclerData, string>
{
    [SerializeField]
    private TMP_Text _text = null;
    
    /// <summary>
    /// Called when the end-cap gets bound
    /// </summary>
    public override void OnFetchedFromRecycling()
    {
        // Do nothing
    }
    
    /// <summary>
    /// Called when the end-cap gets recycled
    /// </summary>
    public override void OnSentToRecycling()
    {
        // Do nothing
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            _text.text = _text.text + "\n" + _text.text;
            RecalculateDimensions();
        }
    }
}