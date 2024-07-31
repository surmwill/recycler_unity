using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// The endcap to a recycler: an entry different than all the others, appearing at the very end of the content.
    /// The unique logic could, for example, be used to fetch the next set of (normal) entries from a database.
    /// </summary>
    public abstract class RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> : MonoBehaviour where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        /// <summary>
        /// The endcap's RectTransform
        /// </summary>
        public RectTransform RectTransform { get; private set; }
        
        /// <summary>
        /// The state of the endcap: visible, cached, or in the pool.
        /// Valid post-fetching.
        /// </summary>
        public RecyclerScrollRectContentState State { get; private set; }

        /// <summary>
        /// The Recycler this endcap is a part of
        /// </summary>
        protected RecyclerScrollRect<TEntryData, TKeyEntryData> Recycler { get; private set; }

        private void Awake()
        {
            RectTransform = (RectTransform) transform;
            Recycler = GetComponentInParent<RecyclerScrollRect<TEntryData, TKeyEntryData>>();
        }

        /// <summary>
        /// Recalculates the endcap's dimensions.
        /// 
        /// Unless specified, with the endcap coming at the very end if the list, we fix all entries that come before it, preserving the current view of things.
        /// (I.e. if the endcap is at the bottom we grow downwards, and if the endcap is a the top we grow upwards).
        /// </summary>
        protected void RecalculateDimensions(FixEntries? fixEntries = null)
        {
            Recycler.RecalculateEndcapSize(fixEntries);
        }
        
        /// <summary>
        /// Called when the active state of the endcap changes, that is, when it moves from: cached -> visible or visible -> cached.
        /// </summary>
        protected virtual void OnStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
        {
            // Empty
        }

        #region CALLED_BY_PARENT_RECYCLER

        /// <summary>
        /// Called when the endcap is fetched from its pool and becomes active
        /// </summary>
        [CalledByRecycler]
        public virtual void OnFetchedFromPool()
        {
            // Empty   
        }

        /// <summary>
        /// Called when the end-cap gets returned to its pool
        /// </summary>
        [CalledByRecycler]
        public virtual void OnReturnedToPool()
        {
            // Empty
        }

        /// <summary>
        /// Sets the state of the entry
        /// </summary>
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