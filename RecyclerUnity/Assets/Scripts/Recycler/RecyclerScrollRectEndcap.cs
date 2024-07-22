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
        /// Called when the endcap becomes active
        /// </summary>
        protected virtual void OnFetchedFromRecycling(RecyclerScrollRectContentState startActiveState)
        {
            // Empty   
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

        #region CALLED_BY_PARENT_RECYCLER

        /// <summary>
        /// Called when the endcap becomes active
        /// </summary>
        public void FetchFromRecycling()
        {
            OnFetchedFromRecycling(State);
        }

        /// <summary>
        /// Called when the end-cap gets recycled
        /// </summary>
        public virtual void OnSentToRecycling()
        {
            // Empty
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
        
        /// <summary>
        /// Called when the active state of the endcap changes, that is, when it moves from: cached -> visible or visible -> cached.
        /// </summary>
        protected virtual void OnActiveStateChanged(RecyclerScrollRectContentState prevActiveState, RecyclerScrollRectContentState newActiveState)
        {
            // Empty
        }
        
        #endregion
    }
}