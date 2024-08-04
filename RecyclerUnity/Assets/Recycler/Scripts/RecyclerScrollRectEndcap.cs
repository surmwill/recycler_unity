using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// The endcap to a recycler: an entry different than all the others, appearing at the very end of the content.
    /// </summary>
    public abstract class RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> : MonoBehaviour where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        /// <summary>
        /// The endcap's RectTransform.
        /// </summary>
        public RectTransform RectTransform { get; private set; }
        
        /// <summary>
        /// The current state of the endcap.
        /// Note that until fetching is complete, the state will report as in the pool.
        /// </summary>
        public RecyclerScrollRectContentState State { get; private set; }

        /// <summary>
        /// The Recycler this endcap is a part of.
        /// </summary>
        protected RecyclerScrollRect<TEntryData, TKeyEntryData> Recycler { get; private set; }

        private void Awake()
        {
            RectTransform = (RectTransform) transform;
            Recycler = GetComponentInParent<RecyclerScrollRect<TEntryData, TKeyEntryData>>();
        }

        /// <summary>
        /// Called when the endcap updates its dimensions and needs to alert the recycler of its new size.
        /// Note that this triggers a layout rebuild of the endcap, incorporating any changes in its auto-calculated size.
        /// </summary>
        /// <param name="fixEntries">
        /// If we're updating the size of a visible endcap, then we'll either be pushing other entries or creating extra space for other entries to occupy.
        /// This defines how and what entries will get moved. If we're not updating an endcap in the visible window, this is ignored,
        /// and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
        ///
        /// Being positioned at the end of the list, the default null value will fix all the entries that come before it.
        /// That value depends on the orientation of the recycler.
        /// </param>
        protected void RecalculateDimensions(FixEntries? fixEntries = null)
        {
            Recycler.RecalculateEndcapSize(fixEntries);
        }
        
        /// <summary>
        /// Lifecycle method called when the state of the endcap changes.
        /// </summary>
        /// <param name="prevState"> The previous state of the endcap. </param>
        /// <param name="newState"> The current state of the endcap. </param>
        protected virtual void OnStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
        {
            // Empty
        }

        #region CALLED_BY_PARENT_RECYCLER

        /// <summary>
        /// Lifecycle method called by the recycler when the endcap becomes active, being fetched from its pool.
        /// </summary>
        [CalledByRecycler]
        public virtual void OnFetchedFromPool()
        {
            // Empty   
        }

        /// <summary>
        /// Lifecycle method called by the recycler when the endcap gets returned to its pool.
        /// </summary>
        [CalledByRecycler]
        public virtual void OnReturnedToPool()
        {
            // Empty
        }

       
        /// <summary>
        /// Called by the recycler to set the current state of the endcap.
        /// </summary>
        /// <param name="newState"> The current state of the endcap. </param>
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