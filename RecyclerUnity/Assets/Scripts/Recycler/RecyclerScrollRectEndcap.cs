using UnityEngine;

/// <summary>
/// The end-cap to a recycler.
///
/// The functions found in this class are similar to those in <see cref="RecyclerScrollRectEntry{TEntryData}"/>.
/// See that class for more detailed documentation.
///
/// Note that all ScrollRect entries are force expanded to the size of the viewport.
/// </summary>
public abstract class RecyclerScrollRectEndcap<TRecyclerData> : MonoBehaviour
{
    /// <summary>
    /// The end-cap's RectTransform
    /// </summary>
    public RectTransform RectTransform { get; private set; }
    
    /// <summary>
    /// The Recycler the end-cap falls under
    /// </summary>
    protected RecyclerScrollRect<TRecyclerData> Recycler { get; private set; }
    
    private void Awake()
    {
        RectTransform = (RectTransform) transform;
        Recycler = GetComponentInParent<RecyclerScrollRect<TRecyclerData>>();
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
    /// Recalculates the end-caps dimensions
    /// </summary>
    protected void RecalculateDimensions()
    {
        Recycler.RecalculateEndcapSize();
    }
}