using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Transform = UnityEngine.Transform;

namespace RecyclerScrollRect
{
    /// <summary>
    /// A Recycler.
    ///
    /// If you have a long list of data to render (say a 1000 text message conversation) it makes no sense to render the entire
    /// conversation, as the entire conversation cannot fit on-screen. Instead you only render the chunk of the conversation that
    /// can fit on-screen. When a message gets scrolled off-screen, that same message is not discarded, but re-used and re-bound
    /// to new message data - the next message that we are scrolling into on-screen (i.e. the message is recycled).
    ///
    /// There are 3 main parts.
    ///
    /// 1.) Your data (a text message)
    /// 2.) An entry prefab that can gets bound to your data (the text message bubble)
    /// 3.) The recycler which manages displaying and recycling all entries (the sum total conversation you scroll through)
    /// 
    /// Steps:
    /// 1.) Create a normal C# class containing your data, ensuring it implements IRecyclerScrollRectData (i.e. supplies a unique key).
    /// Ex: A class containing a text message.
    /// 
    /// 2.) Create an entry prefab to display your data, adding a RecyclerScrollRectEntry component at its root.
    /// This component contains lifecycle methods that bind your data to the prefab when it's ready to become active on-screen.
    /// You implement these methods to define how exactly the binding works.
    /// Ex: A text message bubble prefab. The bubble sets its text component to the whatever message it's being bound to.
    ///
    /// 3.) Add a RecyclerScrollRect component to a RectTransform and serialize your entry prefab.
    ///
    /// 4.) Append, prepend, or insert your data to the RecyclerScrollRect.
    /// The Recycler handles what data gets shown at what time, and you have defined how that data binds to each entry.
    /// Happy scrolling!
    ///
    /// 5.) Optionally create an endcap prefab that looks/operates differently than all the other entries, appearing at the very end of the list.
    /// Add a RecyclerScrollRectEndcap component to its root, and serialize that prefab in your RecyclerScrollRect.
    /// Ex: A loading indicator that fetches the next 50 text messages from the database, and appends them. 
    ///
    /// See full documentation at: https://github.com/surmwill/recycler_unity
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public abstract partial class RecyclerScrollRect<TEntryData, TKeyEntryData> : ScrollRect, IPointerDownHandler where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        private const float DefaultScrollSpeedViewportsPerSecond = 0.5f;
        private const RecyclerPosition DefaultAppendTo = RecyclerPosition.Bot;

        [Header("Recycler")]
        [Tooltip("The prefab which your data gets bound to.")]
        [SerializeField]
        private RecyclerScrollRectEntry<TEntryData, TKeyEntryData> _recyclerEntryPrefab = null;

        [Tooltip("The number of cached entries waiting just above and just below the visible entries to smoothly scroll into.")]
        [SerializeField]
        private int _numCachedAtEachEnd = 2;

        [Tooltip("The direction appended entries get added to.")]
        [SerializeField]
        private RecyclerPosition _appendTo = DefaultAppendTo;

        [Tooltip("The transform under which our entries waiting to be bound/rebound wait.")]
        [Header("Pool")]
        [SerializeField]
        private RectTransform _poolParent = null;

        [Tooltip("The starting number of entries waiting to be bound, so we don't need to freshly instantiate everything at runtime.")]
        [SerializeField]
        private int _poolSize = 15;

        [Header("Endcap (optional)")]
        [Tooltip("The endcap which gets appended at the very end of your entries.")]
        [SerializeField]
        private RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> _endcapPrefab = null;

        [Tooltip("The transform under which the endcap waits to become an active part of the entry list.")]
        [SerializeField]
        private RectTransform _endcapParent = null;

        [Tooltip("A reference to the endcap itself. Read-only and created when the endcap prefab gets serialized.")]
        [ReadOnly]
        [SerializeField]
        private RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> _endcap = null;

        [Header("Extra")]
        [Tooltip("On mobile, the target frame rate is often lower than technically possible to preserve battery, but a higher frame rate will result in smoother scrolling.")]
        [SerializeField]
        private bool _setTargetFrameRateTo60 = true;

        [Tooltip("Used for debugging. Performs sanity checks in the editor, ensuring for example, that we aren't skipping indices.")]
        [SerializeField]
        private bool _debugPerformEditorChecks = true;

        /// <summary>
        /// Called after the Recycler's scroll has been handled in LateUpdate and we have a final set of entries on-screen for this frame.
        /// Unless the user performs a manual operation (append/prepend/insert/delete) after this, the entries will remain where they are
        /// and can queried under this assumption (see ActiveEntries and ActiveEntriesWindow)
        /// </summary>
        public event Action OnRecyclerUpdated;

        /// <summary>
        /// The data being bound to the entries
        /// </summary>
        public IReadOnlyList<TEntryData> DataForEntries => _dataForEntries;

        /// <summary>
        /// The currently active entries, visible and cached. The key is their index. 
        /// </summary>
        public IReadOnlyDictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> ActiveEntries => _activeEntries;

        /// <summary>
        /// Contains information about the ranges of indices that are currently visible or cached.
        /// You can use the index information here to know what keys are in ActiveEntries without iterating through the entire dictionary. 
        /// </summary>
        public IRecyclerScrollRectActiveEntriesWindow ActiveEntriesWindow => _activeEntriesWindow;

        /// <summary>
        /// The endcap (if it exists - it is optional)
        /// </summary>
        public RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> Endcap => _endcap;

        private bool IsZerothEntryAtTop => _appendTo == RecyclerPosition.Bot;

        private RecyclerPosition StartCachePosition => IsZerothEntryAtTop ? RecyclerPosition.Top : RecyclerPosition.Bot;

        private RecyclerPosition EndCachePosition => IsZerothEntryAtTop ? RecyclerPosition.Bot : RecyclerPosition.Top;

        private readonly List<TEntryData> _dataForEntries = new();
        private readonly Dictionary<TKeyEntryData, int> _entryKeyToCurrentIndex = new();

        private Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> _activeEntries = new();
        private readonly RecycledEntries<TEntryData, TKeyEntryData> _recycledEntries = new();
        private readonly Queue<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> _unboundEntries = new();

        private RecyclerScrollRectActiveEntriesWindow _activeEntriesWindow;

        private Vector2 _nonFilledScrollRectPivot;

        private Coroutine _scrollToIndexCoroutine;
        private int? _currScrollingToIndex;

        private BoxCollider _viewportCollider;
        
        private readonly LinkedList<int> _toRecycleEntries = new();
        private readonly LinkedList<int> _newCachedStartEntries = new();
        private readonly LinkedList<int> _newCachedEndEntries = new();
        private LinkedList<int> _updateStateOfEntries = new();

        protected override void Awake()
        {
            base.Awake();

            // The base ScrollRect has [ExecuteAlways] but the recycler does not work as such
            if (!Application.isPlaying)
            {
                return;
            }

            // On mobile, the target frame rate is often lower than technically possible to preserve battery, but a
            // higher frame rate will result in smoother scrolling.
            if (_setTargetFrameRateTo60)
            {
                Application.targetFrameRate = 60;
            }

            // While non-fullscreen, the pivot decides how the content gets aligned in the viewport
            _nonFilledScrollRectPivot = content.pivot;

            // Keeps track of what indices are visible, and subsequently which indices are cached
            _activeEntriesWindow = new RecyclerScrollRectActiveEntriesWindow(_numCachedAtEachEnd);

            // All the entries in the pool are initially unbound
            RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry = null;
            foreach (Transform _ in _poolParent.Children().Where(t => t.TryGetComponent(out entry)))
            {
                _unboundEntries.Enqueue(entry);
            }

            // Collider to detect what is on/offscreen
            _viewportCollider = viewport.GetComponent<BoxCollider>();
        }

        /// <summary>
        /// Inserts an entry at the given index. Existing entries' indices will be shifted like a list insertion.
        /// 
        /// Note that if the entry is not on-screen then FixEntries will be ignored; we will automatically choose
        /// the value of FixEntries that only pushes other off-screen entries, preserving the view of whatever's on-screen. 
        /// </summary>
        public void InsertAtIndex(int index, TEntryData entryData, FixEntries fixEntries = FixEntries.Below)
        {
            if (index < 0 || index > _dataForEntries.Count)
            {
                throw new ArgumentException($"index \"{index}\" must be >= 0 and <= the length of data \"{_dataForEntries.Count}\"");
            }

            // Shift indices
            InsertDataForEntryAt(index, entryData);

            // If the index isn't currently active, we don't need to create the entry, it will be created when we scroll to it
            if (!_activeEntriesWindow.Contains(index))
            {
                return;
            }

            // Find the proper place in the hierarchy for the entry
            int siblingIndex = IsZerothEntryAtTop ? 0 : content.childCount;
            foreach (Transform entryTransform in content)
            {
                RecyclerScrollRectEntry<TEntryData, TKeyEntryData> activeEntry = entryTransform.GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>();
                if (activeEntry != null && activeEntry.Index == index - 1)
                {
                    siblingIndex = activeEntry.transform.GetSiblingIndex() + (IsZerothEntryAtTop ? 1 : 0);
                }
            }

            // Create the entry
            if (_activeEntriesWindow.IsInStartCache(index))
            {
                CreateAndAddEntry(index, siblingIndex, RecyclerScrollRectContentState.ActiveInStartCache, StartCachePosition == RecyclerPosition.Top ? FixEntries.Below : FixEntries.Above);
            }
            else if (_activeEntriesWindow.IsInEndCache(index))
            {
                CreateAndAddEntry(index, siblingIndex, RecyclerScrollRectContentState.ActiveInEndCache, EndCachePosition == RecyclerPosition.Top ? FixEntries.Below : FixEntries.Above);
            }
            else
            {
                CreateAndAddEntry(index, siblingIndex, RecyclerScrollRectContentState.ActiveVisible, fixEntries);
            }

            // Adding the entry shifted things around, possibly pushing things offscreen. Recalculate what entries are active
            RecalculateActiveEntries();
        }

        /// <summary>
        /// Inserts an element at the index corresponding to the given key. Existing entries' indices will be shifted like a list insertion.
        ///
        /// Note that if the entry is not on-screen then FixEntries will be ignored; we will automatically choose
        /// the value of FixEntries that only pushes other off-screen entries, preserving the view of wwhatever's on-screen. 
        /// </summary>
        public void InsertAtKey(TKeyEntryData insertAtKey, TEntryData entryData, FixEntries fixEntries = FixEntries.Below)
        {
            InsertAtIndex(GetCurrentIndexForKey(insertAtKey), entryData, fixEntries);
        }

        /// <summary>
        /// Inserts elements at the given index. Existing entries' indices will be shifted like a list insertion.
        ///
        /// Note that if the entry is not on-screen then FixEntries will be ignored; we will automatically choose
        /// the value of FixEntries that only pushes other off-screen entries, preserving the view of whatever's on-screen. 
        /// </summary>
        public void InsertRangeAtIndex(int index, IEnumerable<TEntryData> entryData, FixEntries fixEntries = FixEntries.Below)
        {
            foreach ((TEntryData entry, int i) in entryData.ZipWithIndex())
            {
                InsertAtIndex(index + i, entry, fixEntries);
            }
        }

        /// <summary>
        /// Inserts elements at the index corresponding to the given key. Existing entries' indices will be shifted like a list insertion.
        ///
        /// Note that if the entry is not on-screen then FixEntries will be ignored; we will automatically choose
        /// the value of FixEntries that only pushes other off-screen entries, preserving the view of whatever's on-screen. 
        /// </summary>
        public void InsertRangeAtKey(TKeyEntryData insertAtKey, IEnumerable<TEntryData> entryData, FixEntries fixEntries = FixEntries.Below)
        {
            InsertRangeAtIndex(GetCurrentIndexForKey(insertAtKey), entryData, fixEntries);
        }

        /// <summary>
        /// Removes an element at the given index. Existing entries' indices will be shifted like a list removal.
        ///
        /// Note that if the entry is not on-screen then FixEntries will be ignored; we will automatically choose
        /// the value of FixEntries that only pushes other off-screen entries, preserving the view of whatever's on-screen. 
        /// </summary>
        public void RemoveAtIndex(int index, FixEntries fixEntries = FixEntries.Below)
        {
            if (index < 0 || index >= _dataForEntries.Count)
            {
                throw new ArgumentException($"index \"{index}\" must be >= 0 and < the length of data \"{_dataForEntries.Count}\"");
            }

            if (index == _currScrollingToIndex)
            {
                StopScrollToIndexCoroutine();
            }

            // Recycle the entry if it exists in the scene
            bool shouldRecycle = _activeEntriesWindow.Contains(index);
            if (shouldRecycle)
            {
                SendToRecycling(_activeEntries[index], fixEntries);
            }

            // Unbind the entry in recycling
            if (_recycledEntries.Entries.TryGetValue(index, out RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry))
            {
                entry.UnbindIndex();
                _recycledEntries.Remove(index);
                _unboundEntries.Enqueue(entry);
            }

            // Shift indices
            RemoveDataForEntryAt(index);

            // Deleting the entry shifted things around, possibly opening up space for new on-screen entries. Recalculate what entries are active
            if (shouldRecycle)
            {
                RecalculateActiveEntries();
            }
        }

        /// <summary>
        /// Removes an element with the given key. Existing entries' indices will be shifted like a list removal.
        ///
        /// Note that if the entry is not on-screen then FixEntries will be ignored; we will automatically choose
        /// the value of FixEntries that only pushes other off-screen entries, preserving the view of whatever's on-screen. 
        /// </summary>
        public void RemoveAtKey(TKeyEntryData removeAtKey, FixEntries fixEntries = FixEntries.Below)
        {
            RemoveAtIndex(GetCurrentIndexForKey(removeAtKey), fixEntries);
        }

        /// <summary>
        /// Removes elements at the given index. Existing entries' indices will be shifted like a list removal.
        ///
        /// Note that if the entry is not on-screen then FixEntries will be ignored; we will automatically choose
        /// the value of FixEntries that only pushes other off-screen entries, preserving the view of whatever's on-screen. 
        /// </summary>
        public void RemoveRangeAtIndex(int index, int count, FixEntries fixEntries = FixEntries.Below)
        {
            for (int i = index + count - 1; i >= index; i--)
            {
                RemoveAtIndex(index, fixEntries);
            }
        }

        /// <summary>
        /// Removes elements at the index corresponding to the given key. Existing entries' indices will be shifted like a list removal.
        ///
        /// Note that if the entry is not on-screen then FixEntries will be ignored; we will automatically choose
        /// the value of FixEntries that only pushes other off-screen entries, preserving the view of whatever's on-screen. 
        /// </summary>
        public void RemoveRangeAtKey(TKeyEntryData removeAtKey, int count, FixEntries fixEntries = FixEntries.Below)
        {
            RemoveRangeAtIndex(GetCurrentIndexForKey(removeAtKey), count, fixEntries);
        }

        /// <summary>
        /// Each piece of entry data is referenced by its index.
        /// When we insert/remove entry data, indices possibly shift, and we need to update any data structure that references those indices to also shift.
        /// </summary>
        private void ShiftIndices(int startIndex, int shiftAmount)
        {
            // Shift our active entries
            Dictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> shiftedActiveEntries = new();

            foreach ((int index, RecyclerScrollRectEntry<TEntryData, TKeyEntryData> activeEntry) in _activeEntries)
            {
                int shiftedIndex = index + (index >= startIndex ? shiftAmount : 0);
                if (shiftedIndex != index)
                {
                    activeEntry.SetIndex(shiftedIndex);
                }

                shiftedActiveEntries[shiftedIndex] = activeEntry;
            }

            _activeEntries = shiftedActiveEntries;

            // Shift our recycled entries
            _recycledEntries.ShiftIndices(startIndex, shiftAmount);

            // Shift the index each key maps to
            for (int i = startIndex; i < _dataForEntries.Count; i++)
            {
                _entryKeyToCurrentIndex[_dataForEntries[i].Key] += shiftAmount;
            }

            // Shift the entry we are currently scrolling to
            if (_currScrollingToIndex.HasValue && _currScrollingToIndex.Value >= startIndex)
            {
                _currScrollingToIndex += shiftAmount;
            }

            // If we are in the midst of updating what entries should be active, shift what we are, or going to be, recycling and adding to the caches
            ShiftLinkedList(_toRecycleEntries);
            ShiftLinkedList(_newCachedStartEntries);
            ShiftLinkedList(_newCachedEndEntries);
            ShiftLinkedList(_updateStateOfEntries);

            void ShiftLinkedList(LinkedList<int> indices)
            {
                LinkedListNode<int> current = indices.First;
                while (current != null)
                {
                    current.Value += current.Value >= startIndex ? shiftAmount : 0;
                    current = current.Next;
                }
            }
        }

        /// <summary>
        /// Appends entries to the end of the recycler.
        /// </summary>
        public void AppendEntries(IEnumerable<TEntryData> entries)
        {
            if (entries?.Any() ?? false)
            {
                InsertDataForEntriesAt(_dataForEntries.Count, new List<TEntryData>(entries));
                RecalculateActiveEntries();
            }
        }

        /// <summary>
        /// Prepends entries to the start of the recycler. Existing entries' indices will be shifted like a list insertion.
        /// </summary>
        public void PrependEntries(IEnumerable<TEntryData> entries)
        {
            if (entries?.Any() ?? false)
            {
                InsertDataForEntriesAt(0, new List<TEntryData>(entries.Reverse()));
                RecalculateActiveEntries();
            }
        }

        protected override void LateUpdate()
        {
            // Handles scrolling
            base.LateUpdate();

            // Ensure our hierarchy with its components are set up properly
            #if UNITY_EDITOR
            EditorSetViewportColliderDimensions();
            EditorCheckRootEntriesComponents();
            #endif

            // The base ScrollRect has [ExecuteAlways] but the recycler does not work as such
            if (!Application.isPlaying)
            {
                return;
            }

            // Update what should be in our start or end cache
            RecalculateActiveEntries();

            // We now have the final set of entries in their correct positions for this frame.
            // Give the user the opportunity to query/operate on them knowing they won't shift.
            OnRecyclerUpdated?.Invoke();

            // Sanity checks
            #if UNITY_EDITOR

            if (_debugPerformEditorChecks)
            {
                DebugCheckWindow();
                DebugCheckWindowAlignment();

                DebugCheckDuplicates();
                DebugCheckOrdering();

                DebugCheckIndexToKeyMapping();
                DebugCheckKeyToIndexMapping();
            }

            #endif
        }

        /// <summary>
        /// Determines what entries are visible, which are not, and what entries need to be in the start and end caches.
        /// Creates and recycles entries accordingly.
        /// </summary>
        private void RecalculateActiveEntries()
        {
            // Check which entries are visible, which are not, and what entries need to be in the start/end caches
            UpdateVisibility();
            
            // If the range of active entries (visible and cached) has not changed, there is nothing more to do
            if (!_activeEntriesWindow.IsDirty)
            {
                return;
            }
            
            // Otherwise we'll need to recycle old entries and add new ones
            LinkedListNode<int> current;
            while (_activeEntriesWindow.IsDirty)
            {
                _activeEntriesWindow.SetNonDirty();
                
                _toRecycleEntries.Clear();
                _newCachedEndEntries.Clear();
                _newCachedEndEntries.Clear();

                // Determine what entries need to be removed (aren't in the cache and aren't visible)
                foreach ((int index, RecyclerScrollRectEntry<TEntryData, TKeyEntryData> _) in _activeEntries)
                {
                    if (!_activeEntriesWindow.Contains(index))
                    {
                        _toRecycleEntries.AddLast(index);
                    }
                }

                // Determine what entries need to be added to the start cache
                if (_activeEntriesWindow.StartCacheIndexRange.HasValue)
                {
                    for (int i = _activeEntriesWindow.StartCacheIndexRange.Value.End; 
                         i >= _activeEntriesWindow.StartCacheIndexRange.Value.Start; 
                         i--)
                    {
                        if (!_activeEntries.ContainsKey(i))
                        {
                            _newCachedStartEntries.AddLast(i);
                        }
                    }
                }

                // Determine what entries need to be added to the end cache
                if (_activeEntriesWindow.EndCacheIndexRange.HasValue)
                {
                    for (int i = _activeEntriesWindow.EndCacheIndexRange.Value.Start; 
                         i <= _activeEntriesWindow.EndCacheIndexRange.Value.End; 
                         i++)
                    {
                        if (!_activeEntries.ContainsKey(i))
                        {
                            _newCachedEndEntries.AddLast(i);
                        }
                    }
                }

                // Recycle unneeded entries
                current = _toRecycleEntries.First;
                while (current != null)
                {
                    _toRecycleEntries.RemoveFirst();
                    SendToRecycling(_activeEntries[current.Value]);
                    current = _toRecycleEntries.First;
                }

                // Create new entries in the start cache
                bool isStartCacheAtTop = StartCachePosition == RecyclerPosition.Top;
                int siblingIndexOffset = GetNumConsecutiveNonEntries(isStartCacheAtTop);

                current = _newCachedStartEntries.First;
                while (current != null)
                {
                    _newCachedStartEntries.RemoveFirst();
                    CreateAndAddEntry(current.Value,
                        isStartCacheAtTop ? siblingIndexOffset : content.childCount - siblingIndexOffset,
                        RecyclerScrollRectContentState.ActiveInStartCache,
                        isStartCacheAtTop ? FixEntries.Below : FixEntries.Above);
                    current = _newCachedStartEntries.First;
                }

                // Create new entries in the end cache
                bool isEndCacheAtTop = EndCachePosition == RecyclerPosition.Top;
                siblingIndexOffset = GetNumConsecutiveNonEntries(isEndCacheAtTop);

                current = _newCachedEndEntries.First;
                while (current != null)
                {
                    _newCachedEndEntries.RemoveFirst();
                    CreateAndAddEntry(current.Value,
                        isEndCacheAtTop ? siblingIndexOffset : content.childCount - siblingIndexOffset,
                        RecyclerScrollRectContentState.ActiveInEndCache,
                        isEndCacheAtTop ? FixEntries.Below : FixEntries.Above);
                    current = _newCachedEndEntries.First;
                }

                // Cached entries come just before and just after what is visible. We may have added entries initially as part of the cache, but there's actually room
                // for them to be visible. Thus we'll check if they are visible, and loop fetching the next set entries to be part of the start/end cache if so.
                // This allows us, for example, to initially insert a single entry of many, have that entry fetch more entries, those entries fetch more, and so on and so on,
                // until we have completely filled up the screen and caches. 
                UpdateVisibility();
            }

            // Append an endcap if we are near the last entry, or remove it if not
            UpdateEndcap();
            
            // Update the state of the entries
            _updateStateOfEntries = new LinkedList<int>(ActiveEntriesWindow);
            
            current = _updateStateOfEntries.First;
            while (current != null)
            {
                int entryIndex = current.Value;
                _activeEntries[entryIndex].SetState(GetStateOfEntryWithCurrentIndex(entryIndex));
                current = _updateStateOfEntries.First;
            }
            
            // Update the state of the endcap
            if (_endcap != null)
            {
                _endcap.SetState(GetStateOfEndcap());
            }

            // Returns the number of consecutive non-entries from the top or bottom of the entry list.
            // Used to insert entries in their rightful sibling index, past any endcaps.
            int GetNumConsecutiveNonEntries(bool fromTop)
            {
                int numConsecutiveNonEntries = 0;

                // Find index of the first entry from the top
                if (fromTop)
                {
                    for (int i = 0; i < content.childCount; i++)
                    {
                        if (content.GetChild(i).GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>() == null)
                        {
                            numConsecutiveNonEntries++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                // Find the index of the first entry from the bottom
                else
                {
                    for (int i = content.childCount - 1; i >= 0; i--)
                    {
                        if (content.GetChild(i).GetComponent<RecyclerScrollRectEntry<TEntryData, TKeyEntryData>>() == null)
                        {
                            numConsecutiveNonEntries++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                return numConsecutiveNonEntries;
            }
        }

        /// <summary>
        /// Adds/removes the endcap, dependent on if we are near the last entry
        /// </summary>
        private void UpdateEndcap()
        {
            if (_endcap == null)
            {
                return;
            }

            bool endcapExists = _endcap.gameObject.activeSelf;
            bool shouldEndcapExist = _dataForEntries.Any() && _activeEntriesWindow.Contains(_dataForEntries.Count - 1);

            if (endcapExists == shouldEndcapExist)
            {
                return;
            }

            // Endcap currently exists, but it shouldn't
            if (!shouldEndcapExist)
            {
                RecycleEndcap();
            }
            // Endcap doesn't currently exist, but it should
            else
            {
                _endcap.transform.SetParent(content, false);
                _endcap.gameObject.SetActive(true);
                _endcap.OnFetchedFromRecycling();

                AddToContent(
                    _endcap.RectTransform,
                    IsZerothEntryAtTop ? content.childCount : 0,
                    EndCachePosition == RecyclerPosition.Top ? FixEntries.Below : FixEntries.Above);
            }
        }

        private void RecycleEndcap()
        {
            RemoveFromContent(_endcap.RectTransform, EndCachePosition == RecyclerPosition.Top ? FixEntries.Below : FixEntries.Above).SetParent(_endcapParent, false);
            _endcap.OnSentToRecycling();
        }

        private void CreateAndAddEntry(int dataIndex, int siblingIndex, RecyclerScrollRectContentState state, FixEntries fixEntries = FixEntries.Below)
        {
            if (!TryFetchFromRecycling(dataIndex, out RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry))
            {
                entry = Instantiate(_recyclerEntryPrefab, content);
            }

            // If we are adding to cache but there is still room on-screen, then it is actually visible
            entry.SetState(this.IsScrollable() ? state : RecyclerScrollRectContentState.ActiveVisible);
            
            if (entry.Index != dataIndex)
            {
                entry.BindNewData(dataIndex, _dataForEntries[dataIndex]);
            }
            else
            {
                entry.RebindExistingData();
            }

            AddToContent(entry.RectTransform, siblingIndex, fixEntries);
            _activeEntries[dataIndex] = entry;
        }

        /// <summary>
        /// Updates the range of entries that are currently shown.
        /// As we cache the entries just before and just after what is visible, this also affects the range of what is cached.
        /// </summary>
        private void UpdateVisibility()
        {
            foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in _activeEntries.Values)
            {
                bool isVisible = IsInViewport(entry.RectTransform);
                if (isVisible)
                {
                    EntryIsVisible(entry);
                }
                else
                {
                    EntryIsNotVisible(entry);
                }
            }

            // Visible
            void EntryIsVisible(RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry)
            {
                int entryIndex = entry.Index;

                if (!_activeEntriesWindow.VisibleIndexRange.HasValue)
                {
                    _activeEntriesWindow.VisibleIndexRange = (entryIndex, entryIndex);
                    return;
                }

                (int Start, int End) newVisibleIndices = _activeEntriesWindow.VisibleIndexRange.Value;

                if (entryIndex < _activeEntriesWindow.VisibleIndexRange.Value.Start)
                {
                    newVisibleIndices.Start = entryIndex;
                }

                if (entryIndex > _activeEntriesWindow.VisibleIndexRange.Value.End)
                {
                    newVisibleIndices.End = entryIndex;
                }

                _activeEntriesWindow.VisibleIndexRange = newVisibleIndices;
            }

            // Not visible
            void EntryIsNotVisible(RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry)
            {
                if (!_activeEntriesWindow.VisibleIndexRange.HasValue)
                {
                    return;
                }

                (int Start, int End) newVisibleIndices = _activeEntriesWindow.VisibleIndexRange.Value;
                int entryIndex = entry.Index;
                bool wentOffTop = Vector3.Dot(entry.RectTransform.position - viewport.transform.position, viewport.transform.up) > 0;

                // Note that for any entry to be non-visible there must be at least one other entry pushing it offscreen.
                // This means there's a guaranteed existent entry below/above it and we can be safe adding +/- 1 to our index window bounds
                if (IsZerothEntryAtTop)
                {
                    // Anything off the top means we are scrolling down, away from entry 0, away from lesser indices
                    if (wentOffTop && _activeEntriesWindow.VisibleIndexRange.Value.Start <= entryIndex)
                    {
                        newVisibleIndices.Start = entryIndex + 1;
                    }
                    // Anything off the bot means we are scrolling up, toward entry 0, toward lesser indices
                    else if (!wentOffTop && _activeEntriesWindow.VisibleIndexRange.Value.End >= entryIndex)
                    {
                        newVisibleIndices.End = entryIndex - 1;
                    }
                }
                // Zeroth entry is at the bottom
                else
                {
                    // Anything off the top means we are scrolling down, toward entry 0, toward lesser indices
                    if (wentOffTop && _activeEntriesWindow.VisibleIndexRange.Value.End >= entryIndex)
                    {
                        newVisibleIndices.End = entryIndex - 1;
                    }
                    // Anything off the bottom means we are scrolling up, away from entry 0, away from lesser indices
                    else if (!wentOffTop && _activeEntriesWindow.VisibleIndexRange.Value.Start <= entryIndex)
                    {
                        newVisibleIndices.Start = entryIndex + 1;
                    }
                }

                _activeEntriesWindow.VisibleIndexRange = newVisibleIndices;
            }
        }

        /// <summary>
        /// Resets our position to the beginning of the list of entries
        /// </summary>
        public void ResetToBeginning()
        {
            List<TEntryData> entryData = _dataForEntries.ToList();
            Clear();
            AppendEntries(entryData);
        }

        /// <summary>
        /// Returns the state of the entry at the given index: visible, in the start cache, in the end cache, or in the recycling pool
        /// </summary>
        public RecyclerScrollRectContentState GetStateOfEntryWithCurrentIndex(int index)
        {
            if (index < 0 || index >= _dataForEntries.Count)
            {
                throw new ArgumentException($"index \"{index}\" must be >= 0 and < the length of data \"{_dataForEntries.Count}\"");
            }

            if (_activeEntriesWindow.IsVisible(index))
            {
                return RecyclerScrollRectContentState.ActiveVisible;
            }

            if (_activeEntriesWindow.IsInStartCache(index))
            {
                return RecyclerScrollRectContentState.ActiveInStartCache;
            }

            if (_activeEntriesWindow.IsInEndCache(index))
            {
                return RecyclerScrollRectContentState.ActiveInEndCache;
            }

            return RecyclerScrollRectContentState.InactiveInPool;
        }

        /// <summary>
        /// Returns the state of the entry with a given key: visible, in the start cache, in the end cache, or in the recycling pool
        /// </summary>
        public RecyclerScrollRectContentState GetStateOfEntryWithKey(TKeyEntryData key)
        {
            return GetStateOfEntryWithCurrentIndex(GetCurrentIndexForKey(key));
        }

        /// <summary>
        /// Returns the state of the endcap: visible, in the end cache, or waiting in the pool
        /// </summary>
        public RecyclerScrollRectContentState GetStateOfEndcap()
        {
            if (!_endcap.gameObject.activeSelf)
            {
                return RecyclerScrollRectContentState.InactiveInPool;
            }

            if (IsInViewport(_endcap.RectTransform))
            {
                return RecyclerScrollRectContentState.ActiveVisible;
            }

            return RecyclerScrollRectContentState.ActiveInEndCache;
        }

        /// <summary>
        /// Resets the Recycler to its initial state: no active entries, no active endcap, and no data.
        /// </summary>
        public void Clear()
        {
            // Stop any active dragging
            StopMovementAndDrag();

            // Stop auto-scrolling to an index
            StopScrollToIndexCoroutine();

            // Upon clearing, all entries should return to the pool unbound. We expect (and will check for) this amount of unbound entries
            #if UNITY_EDITOR
            int numTotalBoundEntries = _activeEntries.Count + _recycledEntries.Entries.Count;
            int numTargetUnboundEntries = numTotalBoundEntries + _unboundEntries.Count;
            #endif

            // Recycle all the entries
            foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in _activeEntries.Values.ToList())
            {
                SendToRecycling(entry);
            }

            // Unbind everything
            foreach (RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry in _recycledEntries.Entries.Values.ToList())
            {
                _recycledEntries.Remove(entry.Index);
                entry.UnbindIndex();
                _unboundEntries.Enqueue(entry);
            }

            // Recycle the end-cap if it exists
            if (_endcap != null)
            {
                RecycleEndcap();
            }

            // Clear the data for the entries
            _dataForEntries.Clear();

            // Clear the keys for all the data
            _entryKeyToCurrentIndex.Clear();

            // Reset our window back to one with no entries
            _activeEntriesWindow.Reset();

            // Reset our pivot to whatever its initial value was
            content.pivot = _nonFilledScrollRectPivot;

            // Check that we have returned to the initial state
            #if UNITY_EDITOR

            if (_dataForEntries.Any())
            {
                throw new DataException("The data is supposed to cleared, but there is still some present.");
            }

            if (_entryKeyToCurrentIndex.Any())
            {
                throw new DataException("The data has been cleared. There should be no keys either.");
            }

            if (_activeEntries.Any())
            {
                throw new DataException($"The data has been cleared. We should not have any active entries.");
            }

            if (_activeEntriesWindow.Exists || _activeEntriesWindow.IsDirty || _activeEntriesWindow.ActiveEntriesRange.HasValue)
            {
                throw new DataException($"The data has been cleared and the window should not exist. There's no underlying data to have a window over.");
            }

            if (_recycledEntries.Entries.Any())
            {
                throw new DataException($"After clearing, all entries should return to the pool unbound. There are still {_recycledEntries.Entries.Count} entries in the pool bound.");
            }

            int numMissingUnboundEntries = numTargetUnboundEntries - _unboundEntries.Count;
            if (numMissingUnboundEntries != 0)
            {
                throw new DataException($"After clearing, all entries should return to the pool unbound. Missing {numMissingUnboundEntries} entries.");
            }

            if (_endcap != null && _endcap.gameObject.activeSelf)
            {
                throw new DataException("The data has been cleared. We expect an empty window and therefore the endcap should not be active.");
            }

            if (_currScrollingToIndex.HasValue || _scrollToIndexCoroutine != null)
            {
                throw new DataException("The data has been cleared. We should not be auto-scrolling to an index.");
            }

            #endif
        }

        private void StopMovementAndDrag()
        {
            OnEndDrag(new PointerEventData(EventSystem.current));
            StopMovement();
        }

        private void SendToRecycling(RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry, FixEntries fixEntries = FixEntries.Below)
        {
            // Handle the GameObject
            RectTransform entryTransform = entry.RectTransform;
            RemoveFromContent(entryTransform, fixEntries);
            entryTransform.SetParent(_poolParent, false);

            // Mark the entry for re-use
            if (_recycledEntries.Entries.ContainsKey(entry.Index))
            {
                throw new InvalidOperationException("We should not have two copies of the same entry in recycling; we only need one.");
            }

            _recycledEntries.Add(entry.Index, entry);

            // Bookkeeping
            _activeEntries.Remove(entry.Index);
            
            // Update the state
            entry.SetState(RecyclerScrollRectContentState.InactiveInPool);

            // Callback
            entry.OnRecycled();
        }

        private bool TryFetchFromRecycling(int entryIndex, out RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry)
        {
            entry = null;

            // First try to use the equivalent already bound entry waiting in recycling
            if (_recycledEntries.Entries.TryGetValue(entryIndex, out entry))
            {
                _recycledEntries.Remove(entryIndex);
            }
            // Then try to use an unbound entry
            else if (_unboundEntries.TryDequeue(out entry))
            {
            }
            // Then try and use the bound entry in recycling that's been there the longest
            else if (_recycledEntries.Entries.Any())
            {
                (int firstIndex, RecyclerScrollRectEntry<TEntryData, TKeyEntryData> firstEntry) = _recycledEntries.GetOldestEntry();
                entry = firstEntry;
                _recycledEntries.Remove(firstIndex);
            }
            // If all else fails, we'll have to instantiate something new
            else
            {
                return false;
            }

            // Note: if we are using a Canvas with "Screen Space - Camera" then previously recycled entries could have a different z based on the position 
            // of the canvas at the time they were recycled. Reset this back to 0 to align with the current Canvas position.
            entry.transform.SetParent(content, false);
            entry.transform.localPosition = entry.transform.localPosition.WithZ(0f);
            entry.gameObject.SetActive(true);

            return true;
        }

        /// <summary>
        /// Adds a child under the (parent) content. This is not straightforward.
        ///
        /// The root of all of entries is a VerticalLayoutGroup with a ContentSizeFitter. Every time an entry is added, removed,
        /// or resized we need to trigger a recalculation of the size of the entire list. This beckons problems.
        ///
        /// 1.) Performance problems: VerticalLayoutGroup size recalculations propagate. If a child entry also has a VerticalLayoutGroup
        /// then it recalculates its size (going down its subtree) and reports that back to the root. Likely our entries don't change
        /// size that often and this is wasted recalculation. Instead, except during explicitly defined times (binding, manual size recalculation calls),
        /// we disable all LayoutGroups of all the children. This cuts the propagation.
        ///
        /// Importantly, we still allow things to be auto-sized by enabling these components during binding and manual size recalculation calls: 
        /// we enable any LayoutGroups and ContentSizeFitters on the child during this time, trigger a layout recalculation of just that child
        /// which sets its RectTransform values accordingly, then disable those components and treat the child like any other plain RectTransform.
        ///
        /// 2.) Because of the above, LayoutGroups and ContentSizeFitters are disabled on children almost all of the time. If the root of all entries
        /// ControlsChildSize Width/Height then we will get entries with 0 height and 0 width. With the components disabled, this is dimensions they report.
        /// Enabling them during size recalculation re-introduces the performance issues. Thus the root of all entries cannot ControlChildSize Width/Height.
        ///
        /// (Note: upon further thought, we may be temped to check ControlChildSize Width and ChildForceExpand Width. If we're force expanding the width, this
        /// does not care about any disabled components reporting 0 values as we don't care what they report; we simply set it to the maximum width. However,
        /// merely checking ControlChildSize incurs a performance cost, including GetComponent calls. It is easier just to not ControlChildSize.) 
        /// </summary>
        private void AddToContent(RectTransform child, int siblingIndex, FixEntries fixEntries = FixEntries.Below)
        {
            // Ensure proper hierarchy
            child.SetParent(content, false);
            child.SetSiblingIndex(siblingIndex);

            // Force expand the width (as we cannot do so through the root VerticalLayoutGroup without also controlling the child size).
            // We assume this is the desired behaviour of most recyclers.
            (child.anchorMin, child.anchorMax) = (Vector2.one * 0.5f, Vector2.one * 0.5f);
            child.sizeDelta = child.sizeDelta.WithX(viewport.rect.width);

            // Calculate the height of the child
            Behaviour[] layoutBehaviours = LayoutUtilities.GetLayoutBehaviours(child.gameObject, true);
            SetBehavioursEnabled(layoutBehaviours, true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(child);
            SetBehavioursEnabled(layoutBehaviours, false);

            // Calculate the change in parent size given the child's size
            RecalculateContentSize(fixEntries);
        }

        private RectTransform RemoveFromContent(RectTransform child, FixEntries fixEntries = FixEntries.Below)
        {
            // If the child is not visible then shrink in the direction which keeps it off screen and preserves the currently visible entries
            if (!IsInViewport(child))
            {
                fixEntries = IsAboveViewport(child) ? FixEntries.Below : FixEntries.Above;
            }

            // Remove the child and recalculate the parent's size
            child.gameObject.SetActive(false);
            RecalculateContentSize(fixEntries);

            return child;
        }

        /// <summary>
        /// Called when a child has updated its dimensions, and needs to alert the parent Recycler of its new size
        /// </summary>
        private void RecalculateContentChildSize(RectTransform contentChild, FixEntries fixEntries = FixEntries.Below)
        {
            // If the child is not visible then grow in the direction which keeps it off screen and preserves the currently visible entries
            if (!IsInViewport(contentChild))
            {
                fixEntries = IsAboveViewport(contentChild) ? FixEntries.Below : FixEntries.Above;
            }

            // Calculate the height of the child
            Behaviour[] layoutBehaviours = LayoutUtilities.GetLayoutBehaviours(contentChild.gameObject, true);
            SetBehavioursEnabled(layoutBehaviours, true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentChild);
            SetBehavioursEnabled(layoutBehaviours, false);

            // Calculate the change in parent size given the change in the child's size
            RecalculateContentSize(fixEntries);
        }

        /// <summary>
        /// Called when an entry has updated its dimensions, and needs to alert the parent Recycler of its new size.
        ///
        /// Note that if the entry is not on-screen then FixEntries will be ignored; we will automatically choose
        /// the value of FixEntries that only pushes other off-screen entries, preserving the view of whatever's on-screen. 
        /// </summary>
        public void RecalculateEntrySize(RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry, FixEntries fixEntries = FixEntries.Below)
        {
            RecalculateContentChildSize(entry.RectTransform, fixEntries);
        }

        /// <summary>
        /// Called when the endcap has updated its dimensions, and needs to alert the parent Recycler of its new size
        /// </summary>
        public void RecalculateEndcapSize(FixEntries? fixEntries = null)
        {
            RecalculateContentChildSize(_endcap.RectTransform, fixEntries ?? (EndCachePosition == RecyclerPosition.Bot ? FixEntries.Above : FixEntries.Below));
        }

        /// <summary>
        /// Recalculates the size of the ScrollRect's content, reflecting any size changes of its elements.
        /// 
        /// A ScrollRect with dynamic content has 2 problems:
        ///
        /// 1.) Inserting/removing an element will push around the other elements, causing things to jump around on-screen.
        /// We can control how things are pushed around by setting the pivot. For example, a pivot with y = 1 will cause any size
        /// changes to come off the bottom of the RectTransform; a pivot with y = 0.5 will cause any size changes to come equally of
        /// the top and bottom of RectTransform; and a pivot with y = 0 will cause any size changes to come off the top of the RectTransform.
        ///
        /// If we are appending elements to the bottom for example, we can preserve our current view of things by setting the pivot to y = 1. The size
        /// increase will be added to the bottom, not shifting our current view of things, but possibly now allowing it to scroll down farther to see
        /// the appended elements.
        ///
        /// If we are inserting elements directly into the viewport then inevitably the current view of elements will need to shift around to make
        /// space for the new one. The user then chooses how things will get shifted around.
        ///
        /// 2.) Inserting/removing an element will cause any held drags to jump. ScrollRects calculate their scroll based on the start drag anchored position and the
        /// current drag anchored position. If the content size changes then the previous anchored positions will be defined relative to a differently sized ScrollRect.
        /// For example, if we started our scroll on element 5, scrolled down to element 10, then inserted a new element 6, we'd need to add a value to the offset equal
        /// to the size of element 6 to stay on element 10. However, there is no way to add this offset directly; instead, we move the pivot, where the start drag happened,
        /// equal to the offset.
        /// </summary>
        private void RecalculateContentSize(FixEntries fixEntries)
        {
            // Initial state
            Vector2 initPivot = content.pivot;
            float initY = content.anchoredPosition.y;

            // Define how the size change will come off the RectTransform, then incorporate the size change
            content.SetPivotWithoutMoving(content.pivot.WithY(fixEntries == FixEntries.Below ? 0f : fixEntries == FixEntries.Above ? 1f : 0.5f));
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);

            // ScrollRects act differently if there's not enough content to scroll through in the first place
            if (!this.IsScrollable())
            {
                // With < fullscreen worth of content, the pivot controls where in the viewport the content is centered. Reset it to whatever it was on initialization
                content.pivot = _nonFilledScrollRectPivot;

                // This seems superfluous, but changing the pivot does not recenter things immediately. We trigger the re-centering here by a normalized position call
                normalizedPosition = normalizedPosition.WithY(0f);

                // If we haven't filled up the viewport yet, there's no need to preserve a current scroll (preventing jumps) because we can't scroll in the first place
                return;
            }

            // Maintain our current scroll, preventing jumps, by moving the anchor equal to the size change
            content.SetPivotWithoutMoving(initPivot);
            float diffY = content.anchoredPosition.y - initY;
            content.SetPivotWithoutMoving(content.pivot + Vector2.up * -diffY / content.rect.height);
        }

        /// <summary>
        /// Scrolls to a given index
        /// </summary>
        public void ScrollToIndex(
            int index,
            ScrollToAlignment scrollToAlignment = ScrollToAlignment.EntryMiddle,
            Action onScrollComplete = null,
            float scrollSpeedViewportsPerSecond = DefaultScrollSpeedViewportsPerSecond,
            bool isImmediate = false)
        {
            if (index < 0 || index >= _dataForEntries.Count)
            {
                throw new ArgumentException($"index \"{index}\" must be >= 0 and < the length of data \"{_dataForEntries.Count}\"");
            }

            if (_scrollToIndexCoroutine != null)
            {
                StopScrollToIndexCoroutine();
            }

            _currScrollingToIndex = index;
            _scrollToIndexCoroutine = StartCoroutine(ScrollToIndexInner(scrollToAlignment, onScrollComplete, scrollSpeedViewportsPerSecond, isImmediate));
        }

        /// <summary>
        /// Scrolls to a given key
        /// </summary>
        public void ScrollToKey(
            TKeyEntryData key,
            ScrollToAlignment scrollToAlignment = ScrollToAlignment.EntryMiddle,
            Action onScrollComplete = null,
            float scrollSpeedViewportsPerSecond = DefaultScrollSpeedViewportsPerSecond,
            bool isImmediate = false)
        {
            ScrollToIndex(GetCurrentIndexForKey(key), scrollToAlignment, onScrollComplete, scrollSpeedViewportsPerSecond, isImmediate);
        }

        private IEnumerator ScrollToIndexInner(
            ScrollToAlignment scrollToAlignment, 
            Action onScrollComplete,
            float scrollSpeedViewportsPerSecond, 
            bool isImmediate)
        {
            // Scrolling should not fight existing movement
            StopMovementAndDrag();

            // The position within the child the scroll will center on (ex: middle, top edge, bottom edge)
            float normalizedPositionWithinChild = 0f;
            switch (scrollToAlignment)
            {
                case ScrollToAlignment.EntryMiddle:
                    normalizedPositionWithinChild = 0.5f;
                    break;

                case ScrollToAlignment.EntryTop:
                    normalizedPositionWithinChild = 1f;
                    break;

                case ScrollToAlignment.EntryBottom:
                    normalizedPositionWithinChild = 0f;
                    break;
            }

            float distanceLeftToTravelThisFrame = GetFullDistanceToTravelInThisFrame();
            while (this.IsScrollable())
            {
                int index = _currScrollingToIndex.Value;

                float normalizedScrollDistanceLeftToTravelThisFrame = DistanceToNormalizedScrollDistance(distanceLeftToTravelThisFrame);
                float currNormalizedY = normalizedPosition.y;
                float newNormalizedY = 0f;

                // Scroll through entries until the entry we want is active; then we'll know the exact position to center on
                if (!_activeEntriesWindow.Contains(index))
                {
                    // Scroll toward lesser indices
                    if (index < _activeEntriesWindow.ActiveEntriesRange.Value.Start)
                    {
                        newNormalizedY = Mathf.MoveTowards(currNormalizedY, IsZerothEntryAtTop ? 1 : 0, normalizedScrollDistanceLeftToTravelThisFrame);
                    }
                    // Scroll toward greater indices
                    else if (index > _activeEntriesWindow.ActiveEntriesRange.Value.End)
                    {
                        newNormalizedY = Mathf.MoveTowards(currNormalizedY, IsZerothEntryAtTop ? 0 : 1, normalizedScrollDistanceLeftToTravelThisFrame);
                    }

                    normalizedPosition = normalizedPosition.WithY(newNormalizedY);
                }

                // Find and scroll to the exact position of the now active entry
                else
                {
                    float entryNormalizedY = this.GetNormalizedVerticalPositionOfChild(_activeEntries[index].RectTransform, normalizedPositionWithinChild);

                    newNormalizedY = Mathf.MoveTowards(currNormalizedY, entryNormalizedY, normalizedScrollDistanceLeftToTravelThisFrame);
                    normalizedPosition = normalizedPosition.WithY(newNormalizedY);

                    if (this.IsAtNormalizedPosition(normalizedPosition.WithY(entryNormalizedY)))
                    {
                        break;
                    }
                }

                // We may not have travelled the full desired distance in this iteration as we might need to spawn the next set of active entries (and scroll past them)
                float distanceTravelledInIteration = NormalizedScrollDistanceToDistance(Mathf.Abs(newNormalizedY - currNormalizedY));

                // If we didn't make any progress in our current iteration we must have travelled the full frame distance (or hit the very end of the list)
                if (Mathf.Approximately(distanceTravelledInIteration, 0f))
                {
                    if (!isImmediate)
                    {
                        yield return null;
                    }

                    distanceLeftToTravelThisFrame = GetFullDistanceToTravelInThisFrame();
                }
                // Otherwise there is still distance left to scroll. Spawn the next set of active entries to scroll past
                else
                {
                    distanceLeftToTravelThisFrame -= distanceTravelledInIteration;
                    RecalculateActiveEntries();
                }
            }

            _currScrollingToIndex = null;
            _scrollToIndexCoroutine = null;
            onScrollComplete?.Invoke();

            // Returns the distance we'd like to scroll in a single frame
            float GetFullDistanceToTravelInThisFrame()
            {
                return scrollSpeedViewportsPerSecond * viewport.rect.height * Time.deltaTime;
            }

            // Returns the normalized scroll distance corresponding to a certain non-normalized distance
            float DistanceToNormalizedScrollDistance(float distance)
            {
                return Mathf.InverseLerp(0, content.rect.height - viewport.rect.height, distance);
            }

            // Returns the distance corresponding to scrolling a certain normalized distance
            float NormalizedScrollDistanceToDistance(float normalizedScrollDistance)
            {
                return normalizedScrollDistance * (content.rect.height - viewport.rect.height);
            }
        }

        private void InsertDataForEntryAt(int index, TEntryData entryData)
        {
            InsertDataForEntriesAt(index, new[] { entryData });
        }

        /// <summary>
        /// Inserts data for a new entry in the list, possibly shifting indices.
        ///
        /// Note that each piece of entry data is referenced by its index.
        /// When we insert entry data, indices possibly shift, and we need to update any data structure that references those indices to also shift.
        /// </summary>
        private void InsertDataForEntriesAt(int index, IReadOnlyCollection<TEntryData> entryData)
        {
            if (index < 0 || index > _dataForEntries.Count)
            {
                throw new IndexOutOfRangeException($"Invalid index: {index}. Current data length: {_dataForEntries.Count}");
            }

            // Shift the indices of existing entries that will be affected by the insertion
            ShiftIndices(index, entryData.Count);

            // Add the inserted entries to our key mapping
            foreach ((TEntryData data, int i) in entryData.ZipWithIndex())
            {
                _entryKeyToCurrentIndex[data.Key] = index + i;
            }

            // Actual insertion (and modification) of underlying data
            _activeEntriesWindow.InsertRange(index, entryData.Count);
            _dataForEntries.InsertRange(index, entryData);
        }

        /// <summary>
        /// Removes data for an entry in the list, possibly shifting indices.
        ///
        /// Note that each piece of entry data is referenced by its index.
        /// When we remove entry data, indices possibly shift, and we need to update any data structure that references those indices to also shift.
        /// </summary>
        private void RemoveDataForEntryAt(int index)
        {
            if (index < 0 || index >= _dataForEntries.Count)
            {
                throw new IndexOutOfRangeException($"Invalid index: {index}. Current data length: {_dataForEntries.Count}");
            }

            // Shift the indices of existing entries that will be affected by the deletion
            ShiftIndices(index + 1, -1);

            // Remove the inserted entry from our key mapping
            _entryKeyToCurrentIndex.Remove(_dataForEntries[index].Key);

            // Actual removal (and modification) of underlying data
            _activeEntriesWindow.Remove(index);
            _dataForEntries.RemoveAt(index);

            // If we are in the midst of updating what entries are active, ensure we don't operate on the removed data
            _toRecycleEntries.Remove(index);
            _newCachedStartEntries.Remove(index);
            _newCachedEndEntries.Remove(index);
            _updateStateOfEntries.Remove(index);
        }

        /// <summary>
        /// Handles what happens when the user taps on the Recycler
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (_scrollToIndexCoroutine != null)
            {
                StopScrollToIndexCoroutine();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            #if UNITY_EDITOR
            _tracker.Clear();
            #endif
        }

        /// <summary>
        /// Stops scrolling to an index
        /// </summary>
        public void CancelScrollTo()
        {
            if (_scrollToIndexCoroutine != null)
            {
                StopScrollToIndexCoroutine();
            }
        }

        /// <summary>
        /// Returns the current index of an entry with a given key
        /// </summary>
        public int GetCurrentIndexForKey(TKeyEntryData key)
        {
            return _entryKeyToCurrentIndex[key];
        }

        /// <summary>
        /// Returns the key of an entry at the given current index
        /// </summary>
        public TKeyEntryData GetKeyForCurrentIndex(int index)
        {
            return _dataForEntries[index].Key;
        }

        private void StopScrollToIndexCoroutine()
        {
            _currScrollingToIndex = null;

            if (_scrollToIndexCoroutine != null)
            {
                StopCoroutine(_scrollToIndexCoroutine);
                _scrollToIndexCoroutine = null;
            }
        }

        private bool IsInViewport(RectTransform rectTransform)
        {
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);
            return worldCorners.Any(_viewportCollider.ContainsPoint);
        }

        private bool IsAboveViewport(RectTransform rectTransform)
        {
            return Vector3.Dot(Vector3.ProjectOnPlane(rectTransform.position - viewport.position, viewport.forward), viewport.up) > 0;
        }

        private static void SetBehavioursEnabled(Behaviour[] behaviours, bool isEnabled)
        {
            Array.ForEach(behaviours, l => l.enabled = isEnabled);
        }
    }
}