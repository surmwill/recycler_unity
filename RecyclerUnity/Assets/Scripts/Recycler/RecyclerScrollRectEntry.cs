using UnityEngine;

/// <summary>
/// An entry in the list displayed by a RecyclerScrollRect.
/// 
/// Note that all ScrollRect entries are force expanded to the size of the viewport.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public abstract class RecyclerScrollRectEntry<TEntryData> : MonoBehaviour
{
    /// <summary>
    /// Index for an entry that is not bound to any data
    /// </summary>
    public const int UnboundIndex = -1;

    /// <summary>
    /// This entries' index
    /// </summary>
    public int Index { get; private set; } = UnboundIndex;
    
    /// <summary>
    /// This entries' RectTransform
    /// </summary>
    public RectTransform RectTransform { get; private set; }

    /// <summary>
    /// The data bound to this entry
    /// </summary>
    public TEntryData Data { get; private set; }
    
    /// <summary>
    /// The scroll rect this is a part of
    /// </summary>
    public RecyclerScrollRect<TEntryData> Recycler { get; private set; }

    public RecyclerEntryState RecyclerEntryState
    {
        get
        {
            ISlidingIndexWindow indexWindow = Recycler.IndexWindow;
            
            if (indexWindow.Exists)
            {
                if (indexWindow.IsVisible(Index))
                {
                    return RecyclerEntryState.Visible;
                }
                
                if (indexWindow.IsInStartCache(Index) || indexWindow.IsInEndCache(Index))
                {
                    return RecyclerEntryState.Cached;
                }
            }

            return Index == UnboundIndex ? RecyclerEntryState.PooledUnbound : RecyclerEntryState.PooledBound;
        }
    }

    protected virtual void Awake()
    {
        RectTransform = (RectTransform) transform;
        Recycler = GetComponentInParent<RecyclerScrollRect<TEntryData>>();
        UnbindIndex();
    }

    #region LIFECYCLE_METHODS
    
    /// <summary>
    /// Binds the entry to a new set of data
    /// </summary>
    protected abstract void OnBindNewData(TEntryData entryData);

    /// <summary>
    /// Re-binds the entry to its current set of data (i.e. it was fetched from the cache)
    /// </summary>
    protected abstract void OnRebindExistingData();

    /// <summary>
    /// Called when the entry gets sent to recycling (i.e. it is not actively part of the list, that is, not visible and not cached).
    /// The next call for this entry will either be OnBindNewData or OnRebindExistingData depending on its next binding data.
    /// </summary>
    protected abstract void OnSentToRecycling();

    #endregion

    #region USED_BY_PARENT_RECYCLER
    
    /// <summary>
    /// Binds the entry to a new set of data
    /// </summary>
    public void BindNewData(int index, TEntryData entryData)
    {
        Data = entryData;
        SetIndex(index);
        OnBindNewData(entryData);
    }

    /// <summary>
    /// Re-binds the entry to its current set of data (i.e. it was fetched from the cache)
    /// </summary>
    public void RebindExistingData()
    {
        OnRebindExistingData();
    }

    /// <summary>
    /// Called when the entry gets sent to recycling (i.e. it is not actively part of the list, that is, not visible and not cached).
    /// The next call for this entry will either be OnBindNewData or OnRebindExistingData depending on its next binding data.
    /// </summary>
    public void OnRecycled()
    {
        OnSentToRecycling();        
    }

    /// <summary>
    /// Resets the entry to its default unbound index
    /// </summary>
    public void UnbindIndex()
    {
        SetIndex(UnboundIndex);
    }

    /// <summary>
    /// Sets the index.
    /// Should only be called by the controlling Recycler.
    /// </summary>
    public void SetIndex(int index)
    {
        Index = index;
        gameObject.name = index.ToString();
    }
    
    #endregion

    /// <summary>
    /// Recalculates the dimensions of the entry
    /// </summary>
    protected void RecalculateDimensions(FixEntries fixEntries = FixEntries.Mid)
    {
        Recycler.RecalculateContentChildSize(RectTransform, fixEntries);
    }
}