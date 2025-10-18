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
        /// Whether the endcap is visible in the viewport. Null indicates it is pooled
        /// </summary>
        public bool? IsVisible { get; private set; }

        /// <summary>
        /// The Recycler this endcap is a part of.
        /// </summary>
        protected RecyclerScrollRect<TEntryData, TKeyEntryData> Recycler { get; private set; }

        protected virtual void Awake()
        {
            RectTransform = (RectTransform) transform;
            Recycler = GetComponentInParent<RecyclerScrollRect<TEntryData, TKeyEntryData>>();
        }

        /// <summary>
        /// Called when the endcap needs to update its height in the recycler.
        /// </summary>
        /// <param name="newHeight"> The height to set the endcap to, null if it should be auto-calculated. </param>
        /// <param name="fixEntries">
        /// If we're updating the size of a visible endcap, then we'll either be pushing other entries or creating extra space for other entries to occupy.
        /// This defines how and what entries will get moved. If we're not updating an endcap in the visible window, this is ignored,
        /// and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
        ///
        /// Being positioned at the end of the list, the default null value will fix all the entries that come before it.
        /// </param>
        protected void RecalculateHeight(float newHeight, FixEntries? fixEntries = null)
        {
            Recycler.RecalculateEndcapHeight(newHeight, fixEntries);
        }
        
        /// <summary>
        /// Called when the endcap needs to recalculate its height in the recycler.
        /// </summary>
        /// <param name="fixEntries">
        /// If we're updating the size of a visible endcap, then we'll either be pushing other entries or creating extra space for other entries to occupy.
        /// This defines how and what entries will get moved. If we're not updating an endcap in the visible window, this is ignored,
        /// and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
        ///
        /// Being positioned at the end of the list, the default null value will fix all the entries that come before it.
        /// </param>
        protected void AutoRecalculateHeight(FixEntries? fixEntries = null)
        {
            Recycler.RecalculateEndcapHeight(null, fixEntries);
        }
        
        #region LIFECYCLE_METHODS
        
        /// <summary>
        /// Lifecycle method called when the endcap becomes active, being fetched from its pool.
        /// </summary>
        [CalledByRecycler]
        protected virtual void OnFetchedFromPool()
        {
            // Empty   
        }

        /// <summary>
        /// Lifecycle method called when the endcap gets returned to its pool.
        /// </summary>
        [CalledByRecycler]
        protected virtual void OnReturnedToPool()
        {
            // Empty
        }
        
        /// <summary>
        /// Called when the visibility of the endcap changes as it enters and leaves the viewport
        /// </summary>
        /// <param name="isVisible"> Whether the endcap is visible in the viewport </param>
        /// <param name="isInitial"> Whether this is the initial visible state of the endcap </param> 
        protected virtual void OnVisibilityChanged(bool isVisible, bool isInitial)
        {
            // Empty
        }
        
        #endregion

        #region CALLED_BY_PARENT_RECYCLER

        /// <summary>
        /// Called by the recycler when the endcap gets fetched from the pool
        /// </summary>
        [CalledByRecycler]
        public void FetchFromPool()
        {
            OnFetchedFromPool();
        }

        /// <summary>
        /// Called by the recycler when the endcap gets returned to the pool
        /// </summary>
        [CalledByRecycler]
        public void ReturnToPool()
        {
            if (IsVisible.HasValue)
            {
                SetVisibility(false);   
                SetVisibility(null);
            }
            
            OnReturnedToPool();
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