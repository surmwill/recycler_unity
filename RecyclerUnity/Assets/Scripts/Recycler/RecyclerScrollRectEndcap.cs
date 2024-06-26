using UnityEngine;

/// <summary>
/// The end-cap to a recycler.
/// Note that all ScrollRect entries are force expanded to the size of the viewport.
/// </summary>
public abstract class RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> : MonoBehaviour where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
{
    /// <summary>
    /// The end-cap's RectTransform
    /// </summary>
    public RectTransform RectTransform { get; private set; }
    
    /// <summary>
    /// The Recycler the end-cap falls under
    /// </summary>
    protected RecyclerScrollRect<TEntryData, TKeyEntryData> Recycler { get; private set; }
    
    private void Awake()
    {
        RectTransform = (RectTransform) transform;
        Recycler = GetComponentInParent<RecyclerScrollRect<TEntryData, TKeyEntryData>>();
    }
    
    /// <summary>
    /// Called when the end-cap becomes active (note: can still be offscreen in the cache)
    /// </summary>
    public abstract void OnFetchedFromRecycling();

    /// <summary>
    /// Called when the end-cap gets recycled
    /// </summary>
    public abstract void OnSentToRecycling();

    /// <summary>
    /// Recalculates the endcaps dimensions.
    /// 
    /// Unless specified, as the endcap is at the end, we fix all entries that come before it
    /// (i.e. if the endcap is at the bottom we grow downwards, and if the endcap is a the top we grow upwards)
    /// </summary>
    protected void RecalculateDimensions(FixEntries? fixEntries = null)
    {
        Recycler.RecalculateEndcapSize(fixEntries);
    }
}