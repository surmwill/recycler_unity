using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Base class for all entries displayed in the Recycler (excluding the optional endcap)
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class RecyclerScrollRectEntry<TEntryData, TKeyEntryData> : MonoBehaviour where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        /// <summary>
        /// Index for an entry that is not bound to any data
        /// </summary>
        public const int UnboundIndex = -1;

        /// <summary>
        /// This entry's index
        /// </summary>
        public int Index { get; private set; } = UnboundIndex;

        /// <summary>
        /// This entry's RectTransform
        /// </summary>
        public RectTransform RectTransform { get; private set; }

        /// <summary>
        /// The current data bound to this entry
        /// </summary>
        public TEntryData Data { get; private set; }
        
        /// <summary>
        /// The state of the bound entry: visible, cached, or in the pool.
        /// </summary>
        public RecyclerScrollRectContentState State { get; private set; }

        /// <summary>
        /// The recycler this is entry is a part of
        /// </summary>
        public RecyclerScrollRect<TEntryData, TKeyEntryData> Recycler { get; private set; }

        protected virtual void Awake()
        {
            RectTransform = (RectTransform) transform;
            Recycler = GetComponentInParent<RecyclerScrollRect<TEntryData, TKeyEntryData>>();
            UnbindIndex();
        }

        #region LIFECYCLE_METHODS

        /// <summary>
        /// Binds the entry to a new set of data
        /// </summary>
        protected abstract void OnBindNewData(TEntryData entryData, RecyclerScrollRectContentState onBindState);

        /// <summary>
        /// Rebinds this entry to its existing held data, possibly allowing a resumption of operations instead of a fresh restart
        /// </summary>
        protected virtual void OnRebindExistingData(RecyclerScrollRectContentState onRebindState)
        {
            // Empty
        }

        /// <summary>
        /// Called when the entry gets sent to recycling.
        /// Its data will not be unbound until we know we need to be bind it to different data.
        /// </summary>
        protected virtual void OnSentToRecycling()
        {
            // Empty
        }

        /// <summary>
        /// Called when the active state of an entry changes, that is, when it moves from cached -> visible or visible -> cached
        /// </summary>
        protected virtual void OnActiveStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
        {
            // Empty
        }

        #endregion

        #region USED_BY_PARENT_RECYCLER

        /// <summary>
        /// Binds the entry to a new set of data
        /// </summary>
        public void BindNewData(int index, TEntryData entryData)
        {
            Data = entryData;
            SetIndex(index);
            OnBindNewData(entryData, State);
        }

        /// <summary>
        /// Rebinds this entry to its existing held data, possibly allowing a resumption of operations instead of a fresh restart
        /// </summary>
        public void RebindExistingData()
        {
            OnRebindExistingData(State);
        }

        /// <summary>
        /// Called when the entry gets sent to recycling.
        /// Its data will not be unbound until we know we need to be bind it to different data.
        /// </summary>
        public void OnRecycled()
        {
            State = RecyclerScrollRectContentState.InactiveInPool;
            OnSentToRecycling();
        }

        /// <summary>
        /// Unbinds the entry
        /// </summary>
        public void UnbindIndex()
        {
            SetIndex(UnboundIndex);
        }

        /// <summary>
        /// Sets the index.
        /// </summary>
        public void SetIndex(int index)
        {
            Index = index;
            gameObject.name = index.ToString();
        }

        /// <summary>
        /// Sets the state of the entry
        /// </summary>
        public void SetState(RecyclerScrollRectContentState newState)
        {
            RecyclerScrollRectContentState lastState = State;
            State = newState;
            
            if (lastState != RecyclerScrollRectContentState.InactiveInPool && 
                newState != RecyclerScrollRectContentState.InactiveInPool &&  
                newState != lastState)
            {
                OnActiveStateChanged(lastState, newState);   
            }
        }

        #endregion

        /// <summary>
        /// Alerts the Recycler of size changes, allowing the Recycler to properly display it.
        /// If the entry is auto-sized, this also triggers a auto-size recalculation prior to alerting the Recycler.
        ///
        /// Note that if the entry is not on-screen then FixEntries will be ignored; we will automatically choose
        /// the value of FixEntries that only pushes other off-screen entries, preserving the view of whatever's on-screen. 
        /// </summary>
        protected void RecalculateDimensions(FixEntries fixEntries = FixEntries.Mid)
        {
            Recycler.RecalculateEntrySize(this, fixEntries);
        }
    }
}