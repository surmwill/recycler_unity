using UnityEngine;

namespace com.swill.recycler
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
        /// The recycler this is entry is a part of.
        /// </summary>
        public RecyclerScrollRect<TEntryData, TKeyEntryData> Recycler { get; private set; }
        
        /// <summary>
        /// A unique id representing the GameObject this entry lives on.
        /// </summary>
        public int UidGameObject { get; private set; }

        /// <summary>
        /// Whether the entry is visible in the viewport. Null indicates it is in the recycling pool.
        /// </summary>
        public bool? IsVisible { get; private set; }

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
        /// <param name="newHeight"> The new height the entry should be set to </param>
        /// <param name="fixEntries">
        /// If we're updating the size of a visible entry, then we'll either be pushing other entries or creating extra space for other entries to occupy.
        /// This defines how and what entries will get moved. If we're not updating an entry in the visible window, this is ignored,
        /// and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        protected void RecalculateHeight(float newHeight, FixEntries fixEntries = FixEntries.Mid)
        {
            Recycler.RecalculateEntryHeight(this, newHeight, fixEntries);
        }
        
        /// <summary>
        /// Called when an entry needs to recalculate its height in the recycler.
        /// </summary>
        /// <param name="fixEntries">
        /// If we're updating the size of a visible entry, then we'll either be pushing other entries or creating extra space for other entries to occupy.
        /// This defines how and what entries will get moved. If we're not updating an entry in the visible window, this is ignored,
        /// and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        protected void AutoRecalculateHeight(FixEntries fixEntries = FixEntries.Mid)
        {
            Recycler.RecalculateEntryHeight(this, null, fixEntries);
        }

        #region LIFECYCLE_METHODS

        /// <summary>
        /// Lifecycle method called when the entry becomes active and bound to a new piece of data.
        /// </summary>
        /// <param name="entryData"> The data the entry is being bound to. </param>
        protected abstract void OnBind(TEntryData entryData);

        /// <summary>
        /// Lifecycle method called instead of OnBind when the data to be bound to is the same data that's already bound.
        /// (Entries maintain their data/state when recycled, only losing it when being bound to new data).
        /// </summary>
        protected virtual void OnCachedRebind()
        {
            // Empty
        }

        /// <summary>
        /// Lifecycle method called when the entry gets sent back to the recycling pool.
        /// </summary>
        protected virtual void OnRecycled()
        {
            // Empty
        }
        
        /// <summary>
        /// Called when the visibility of the entry changes as it enters and leaves the viewport
        /// </summary>
        /// <param name="isVisible"> Whether the entry is visible in the viewport </param>
        /// <param name="isInitial"> Whether this is the initial visible state of the entry </param> 
        protected virtual void OnVisibilityChanged(bool isVisible, bool isInitial)
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
            OnBind(entryData);
        }

        /// <summary>
        /// Called by the recycler to rebind the entry to its currently bound data.
        /// </summary>
        [CalledByRecycler]
        public void RebindExistingData()
        {
            OnCachedRebind();
        }

        /// <summary>
        /// Called by the recycler when the entry gets recycled.
        /// </summary>
        [CalledByRecycler]
        public void Recycle()
        {
            if (IsVisible.HasValue)
            {
                SetVisibility(false);   
                SetVisibility(null);
            }
            
            OnRecycled();
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
        /// Called by the recycler to set the entry's index.
        /// </summary>
        [CalledByRecycler]
        public void SetIndex(int index)
        {
            Index = index;
            gameObject.name = index.ToString();
        }

        /// <summary>
        /// Called by the recycler to set the current visible state of the entry
        /// </summary>
        /// <param name="isVisible"> Whether the entry is visible (null indicates the entry is in the pool) </param>
        [CalledByRecycler]
        public void SetVisibility(bool? isVisible)
        {
            bool? lastIsVisible = IsVisible;
            IsVisible = isVisible;

            if (!isVisible.HasValue)
            {
                return;
            }

            if (isVisible.Value != lastIsVisible)
            {
                OnVisibilityChanged(isVisible.Value, !lastIsVisible.HasValue);
            }
        }

        #endregion
    }
}