using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Base class for all entries displayed in the Recycler (excluding the optional endcap)
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class RecyclerScrollRectEntry<TEntryData, TKeyEntryData> : MonoBehaviour
        where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
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
        protected abstract void OnBindNewData(TEntryData entryData);

        /// <summary>
        /// Rebinds this entry to its existing held data, possibly allowing a resumption of operations instead of a fresh restart
        /// </summary>
        protected abstract void OnRebindExistingData();

        /// <summary>
        /// Called when the entry gets sent to recycling.
        /// Its data will not be unbound until we know we need to be bind it to different data.
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
        /// Rebinds this entry to its existing held data, possibly allowing a resumption of operations instead of a fresh restart
        /// </summary>
        public void RebindExistingData()
        {
            OnRebindExistingData();
        }

        /// <summary>
        /// Called when the entry gets sent to recycling.
        /// Its data will not be unbound until we know we need to be bind it to different data.
        /// </summary>
        public void OnRecycled()
        {
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

        #endregion

        /// <summary>
        /// Alerts the Recycler of size changes, allowing the Recycler to properly display it.
        /// If the entry is auto-sized, this also triggers a auto-size recalculation prior to alerting the Recycler.
        /// </summary>
        protected void RecalculateDimensions(FixEntries fixEntries = FixEntries.Mid)
        {
            Recycler.RecalculateEntrySize(this, fixEntries);
        }
    }
}