using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Base class for all entries displayed in the recycler. Contains overridable lifecycle methods to customize their behaviour.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class RecyclerScrollRectEntry<TEntryData, TKeyEntryData> : MonoBehaviour where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        private static int UidGameObjectCounter = 0;
        
        /// <summary>
        /// Index for an entry that is not bound to any data.
        /// </summary>
        public const int UnboundIndex = -1;

        /// <summary>
        /// This current index of the entry (note that indices can shift as things are added and removed).
        /// </summary>
        public int Index { get; private set; } = UnboundIndex;

        /// <summary>
        /// The entry's RectTransform.
        /// </summary>
        public RectTransform RectTransform { get; private set; }

        /// <summary>
        /// The data this entry is currently bound to.
        /// </summary>
        public TEntryData Data { get; private set; }
        
        /// <summary>
        /// The current state of the entry.
        /// Note that until binding/rebinding is complete, the state will report as in the pool.
        /// </summary>
        public RecyclerScrollRectContentState State { get; private set; }

        /// <summary>
        /// The recycler this is entry is a part of.
        /// </summary>
        public RecyclerScrollRect<TEntryData, TKeyEntryData> Recycler { get; private set; }
        
        /// <summary>
        /// A unique id representing the GameObject this entry lives on.
        /// </summary>
        public int UidGameObject { get; private set; }

        protected virtual void Awake()
        {
            UidGameObject = UidGameObjectCounter++;
            RectTransform = (RectTransform) transform;
            Recycler = GetComponentInParent<RecyclerScrollRect<TEntryData, TKeyEntryData>>();
            UnbindIndex();
        }
        
        /// <summary>
        /// Called when an entry needs to update its height in the recycler.
        /// </summary>
        /// <param name="newHeight"> The new height the entry should be set to, null if it should be auto-calculated. </param>
        /// <param name="fixEntries">
        /// If we're updating the size of a visible entry, then we'll either be pushing other entries or creating extra space for other entries to occupy.
        /// This defines how and what entries will get moved. If we're not updating an entry in the visible window, this is ignored,
        /// and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        protected void RecalculateHeight(float? newHeight, FixEntries fixEntries = FixEntries.Mid)
        {
            Recycler.RecalculateEntryHeight(this, newHeight, fixEntries);
        }

        #region LIFECYCLE_METHODS

        /// <summary>
        /// Lifecycle method called when the entry becomes active and bound to a new piece of data.
        /// </summary>
        /// <param name="entryData"> The data the entry is being bound to. </param>
        protected abstract void OnBindNewData(TEntryData entryData);

        /// <summary>
        /// Lifecycle method called instead of OnBindNewData when the data to be bound to is the same data that's already bound.
        /// (Note that entries maintain their state when recycled, only losing it when being bound to new data).
        /// </summary>
        protected virtual void OnRebindExistingData()
        {
            // Empty
        }

        /// <summary>
        /// Lifecycle method called when the entry gets sent back to the recycling pool.
        /// </summary>
        protected virtual void OnSentToRecycling()
        {
            // Empty
        }
        
        /// <summary>
        /// Lifecycle method called when the state of the entry changes.
        /// Note that if entry's previous state was in the pool, the new state is the initial state of the entry post-binding/rebinding.
        /// </summary>
        /// <param name="prevState"> The previous state of the entry. </param>
        /// <param name="newState"> The current state of the entry. </param>
        protected virtual void OnStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
        {
            // Empty
        }

        #endregion

        #region CALLED_BY_PARENT_RECYCLER

        /// <summary>
        /// Called by the recycler to bind the entry to a new set of data.
        /// </summary>
        /// <param name="index"> The index of the entry. </param>
        /// <param name="entryData"> The data for the entry. </param>
        [CalledByRecycler]
        public void BindNewData(int index, TEntryData entryData)
        {
            Data = entryData;
            SetIndex(index);
            OnBindNewData(entryData);
        }

        /// <summary>
        /// Called by the recycler to rebind the entry to its currently bound data.
        /// </summary>
        [CalledByRecycler]
        public void RebindExistingData()
        {
            OnRebindExistingData();
        }

        /// <summary>
        /// Called by the recycler when the entry gets recycled.
        /// </summary>
        [CalledByRecycler]
        public void OnRecycled()
        {
            OnSentToRecycling();
        }

        /// <summary>
        /// Called by the recycler to reset the entry to its default unbound index.
        /// </summary>
        [CalledByRecycler]
        public void UnbindIndex()
        {
            SetIndex(UnboundIndex);
        }

        /// <summary>
        /// Called by the recycler to set the the entry's index.
        /// </summary>
        [CalledByRecycler]
        public void SetIndex(int index)
        {
            Index = index;
            gameObject.name = index.ToString();
        }

        /// <summary>
        /// Called by the recycler to set the current state of the entry.
        /// </summary>
        /// <param name="newState"> The current state of the entry. </param>
        [CalledByRecycler]
        public void SetState(RecyclerScrollRectContentState newState)
        {
            RecyclerScrollRectContentState lastState = State;
            State = newState;
            
            if (newState != lastState)
            {
                OnStateChanged(lastState, newState);   
            }
        }

        #endregion
    }
}