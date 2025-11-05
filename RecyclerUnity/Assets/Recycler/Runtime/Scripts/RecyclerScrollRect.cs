using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using static Swill.Recycler.ViewportHelpers;
using Transform = UnityEngine.Transform;

namespace Swill.Recycler
{
    /// <summary>
    /// A Recycler for efficiently display long lists of data.
    ///
    /// 3 things are needed for a working recycler:
    ///
    /// 1.) Your data, a C# class that implements IRecyclerScrollRectData (i.e. each piece of data has a unique key)
    /// 2.) A prefab that takes your data and display it with the RecyclerScrollRectEntry component
    /// 3.) A Recycler component which, using your prefab and being passed your data, binds your data to the proper GameObject (prefab) while scrolling the list.
    /// 4.) An optional endcap prefab, which appears at the end of the list as its own distinct GameObject with its own distinct behaviour 
    /// 
    /// See full documentation at: https://github.com/surmwill/recycler_unity
    /// </summary>
    public abstract partial class RecyclerScrollRect<TKeyEntryData, TEntryData> : ScrollRectWithDragSensitivity, IPointerDownHandler, IPointerUpHandler where TEntryData : IRecyclerScrollRectData<TKeyEntryData>
    {
        [Header("Recycler")]
        [Tooltip("The prefab which your data gets bound to.")]
        [SerializeField]
        private RecyclerScrollRectEntry<TKeyEntryData, TEntryData> _recyclerEntryPrefab = null;

        [Tooltip("The number of cached entries waiting just above and just below the visible entries to smoothly scroll into.")]
        [SerializeField]
        private int _numCachedAtEachEnd = 2;

        [Tooltip("The orientation of the recycler. Defines the position of the starting entry and where entries get appended.")]
        [SerializeField]
        private RecyclerScrollRectOrientation _orientation = DefaultOrientation;

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
        private RecyclerScrollRectEndcap<TKeyEntryData, TEntryData> _endcapPrefab = null;

        [Tooltip("The transform under which the endcap waits to become an active part of the entry list.")]
        [SerializeField]
        private RectTransform _endcapParent = null;

        [Tooltip("A reference to the endcap itself. Read-only and created when the endcap prefab gets serialized.")]
        [ReadOnly]
        [SerializeField]
        private RecyclerScrollRectEndcap<TKeyEntryData, TEntryData> _endcap = null;

        [Header("Extra")]
        [Tooltip("On mobile, the target frame rate is often lower than technically possible to preserve battery, but a higher frame rate will result in smoother scrolling.")]
        [SerializeField]
        private bool _setTargetFrameRateTo60 = false;

        /// <summary>
        /// Invoked at the end of LateUpdate once scrolling has been handled. 
        /// Here, the current viewport of entries is not expected to change for the remainder of the frame except through manual user calls.
        /// The state of the entries can be queried here without worry of them changing.
        /// </summary>
        public event Action OnRecyclerUpdated;

        /// <summary>
        /// The data being bound to the entries
        /// </summary>
        public IReadOnlyList<TEntryData> DataForEntries => _dataForEntries;

        /// <summary>
        /// The currently active entries: visible and cached. The key is their index. 
        /// </summary>
        public IReadOnlyDictionary<int, RecyclerScrollRectEntry<TKeyEntryData, TEntryData>> ActiveEntries => _activeEntries;

        /// <summary>
        /// Contains information about the current index ranges of active entries.
        /// </summary>
        public IRecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> ActiveEntriesWindow => _activeEntriesWindow;

        /// <summary>
        /// A reference to the endcap, if it exists.
        /// </summary>
        public RecyclerScrollRectEndcap<TKeyEntryData, TEntryData> Endcap => _endcap;

        /// <summary>
        /// The origin of the first position and the direction that entries get appended to
        /// </summary>
        public RecyclerScrollRectOrientation Orientation => _orientation;

        private bool IsEndcapActive => _hasEndcap && _endcap.gameObject.activeSelf;
        
        private const float DefaultScrollSpeedViewportsPerSecond = 1f;
        private const RecyclerScrollRectOrientation DefaultOrientation = RecyclerScrollRectOrientation.BottomToTop;

        private readonly List<TEntryData> _dataForEntries = new();
        private readonly Dictionary<TKeyEntryData, int> _entryKeyToCurrentIndex = new();

        private readonly Dictionary<int, RecyclerScrollRectEntry<TKeyEntryData, TEntryData>> _activeEntries = new();
        private readonly RecycledEntries<TKeyEntryData, TEntryData> _recycledEntries = new();
        private readonly Queue<RecyclerScrollRectEntry<TKeyEntryData, TEntryData>> _unboundEntries = new();
        
        private readonly Dictionary<int, Behaviour[]> _entryGameObjectLayoutBehaviours = new();
        private Behaviour[] _endcapLayoutBehaviours;

        private RecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData> _activeEntriesWindow;

        private DrivenRectTransformTracker _tracker;
        private Vector2 _initPivot;

        private Coroutine _scrollToIndexCoroutine;
        private int? _currScrollingToIndex;
        private int _initFrameRate;
        
        private Canvas _rootCanvas;
        
        private readonly LinkedList<int> _toRecycleEntries = new();
        private readonly LinkedList<int> _newCachedStartEntries = new();
        private readonly LinkedList<int> _newCachedEndEntries = new();
        private LinkedList<int> _updateStateOfEntries = new();

        private bool _hasEndcap;

        private Action _onScrollCancelled;
        private bool _isPressed;

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
                _initFrameRate = Application.targetFrameRate;
                Application.targetFrameRate = 60;
            }
            
            // Root canvas
            _rootCanvas = GetComponent<Canvas>();
            if (_rootCanvas == null)
            {
                _rootCanvas = GetComponentInParent<Canvas>();
            }
            _rootCanvas = _rootCanvas.rootCanvas;

            // Keeps track of what indices are visible, and subsequently which indices are cached
            _activeEntriesWindow = new RecyclerScrollRectActiveEntriesWindow<TKeyEntryData, TEntryData>(this, _numCachedAtEachEnd);

            // All the entries in the pool are initially unbound
            RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry = null;
            foreach (Transform _ in _poolParent.Children().Where(t => t.TryGetComponent(out entry)))
            {
                _unboundEntries.Enqueue(entry);
            }

            // Ensure content's RectTransform is set up correctly
            SetContentTracker();
            
            // Our pivot moves around once we have > full-screen's worth of content, but should be reset when we have <= 
            _initPivot = content.pivot;

            // Cache the endcap's layout behaviours if there are any. These will be disabled when not in use for performance reasons.
            _hasEndcap = _endcap != null;
            if (_hasEndcap)
            {
                _endcapLayoutBehaviours = LayoutUtilities.GetLayoutBehaviours(_endcap.gameObject, true);
            }
        }
        
        /// <summary>
        /// Inserts an entry at the given index. Existing entries' indices will be shifted like a list insertion.
        /// </summary>
        /// <param name="index"> The index to insert the entry at. </param>
        /// <param name="entryData"> The data representing the entry. </param>
        /// <param name="fixEntries">
        /// If we're inserting into the visible window of entries, then we'll need to make some room by pushing some existing entries aside.
        /// This defines how and what entries will get moved. If we're not inserting into the visible window, this is ignored, and the parameter
        /// will be overriden with whatever value only pushes other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        /// <exception cref="ArgumentException"> Thrown when trying to insert at an invalid index. </exception>
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
            
            // Create the entry
            int siblingIndex = GetSiblingIndexForEntry(index);
            if (_activeEntriesWindow.IsInStartCache(index))
            {
                CreateAndAddEntry(index, siblingIndex, Orientation == RecyclerScrollRectOrientation.TopToBottom ? FixEntries.Below : FixEntries.Above);
            }
            else if (_activeEntriesWindow.IsInEndCache(index))
            {
                CreateAndAddEntry(index, siblingIndex, Orientation == RecyclerScrollRectOrientation.TopToBottom ? FixEntries.Above : FixEntries.Below);
            }
            else
            {
                CreateAndAddEntry(index, siblingIndex, fixEntries);
            }

            // Adding the entry shifted things around, possibly pushing things offscreen. Recalculate what entries are active
            RecalculateActiveEntries();
        }

        /// <summary>
        /// Inserts an element at the index corresponding to the given key. Existing entries' indices will be shifted like a list insertion.
        /// </summary>
        /// <param name="insertAtKey"> The key to insert the entry at. </param>
        /// <param name="entryData"> The data representing the entry. </param>
        /// <param name="fixEntries">
        /// If we're inserting into the visible window of entries, then we'll need to make some room by pushing some existing entries aside.
        /// This defines how and what entries will get moved. If we're not inserting into the visible window, this is ignored, and the parameter
        /// will be overriden with whatever value only pushes other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        public void InsertAtKey(TKeyEntryData insertAtKey, TEntryData entryData, FixEntries fixEntries = FixEntries.Below)
        {
            InsertAtIndex(_entryKeyToCurrentIndex[insertAtKey], entryData, fixEntries);
        }

        /// <summary>
        /// Inserts elements at the given index. Existing entries' indices will be shifted like a list insertion.
        /// </summary>
        /// <param name="index"> The index to insert the entries at. </param>
        /// <param name="dataForEntries"> The data for the entries. </param>
        /// <param name="fixEntries">
        /// If we're inserting into the visible window of entries, then we'll need to make some room by pushing some existing entries aside.
        /// This defines how and what entries will get moved. If we're not inserting into the visible window, this is ignored, and the parameter
        /// will be overriden with whatever value only pushes other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        public void InsertRangeAtIndex(int index, IEnumerable<TEntryData> dataForEntries, FixEntries fixEntries = FixEntries.Below)
        {
            foreach ((TEntryData entry, int i) in dataForEntries.ZipWithIndex())
            {
                InsertAtIndex(index + i, entry, fixEntries);
            }
        }

        /// <summary>
        /// Inserts elements at the index corresponding to the given key. Existing entries' indices will be shifted like a list insertion.
        /// </summary>
        /// <param name="insertAtKey"> The key to insert the entries at. </param>
        /// <param name="dataForEntries"> The data for the entries. </param>
        /// <param name="fixEntries">
        /// If we're inserting into the visible window of entries, then we'll need to make some room by pushing some existing entries aside.
        /// This defines how and what entries will get moved. If we're not inserting into the visible window, this is ignored, and the parameter
        /// will be overriden with whatever value only pushes other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        public void InsertRangeAtKey(TKeyEntryData insertAtKey, IEnumerable<TEntryData> dataForEntries, FixEntries fixEntries = FixEntries.Below)
        {
            InsertRangeAtIndex(_entryKeyToCurrentIndex[insertAtKey], dataForEntries, fixEntries);
        }

        /// <summary>
        /// Removes an element at the given index. Existing entries' indices will be shifted like a list removal.
        /// </summary>
        /// <param name="index"> The index of the entry to remove. </param>
        /// <param name="fixEntries">
        /// If we're removing from the visible window of entries, then we'll be creating some extra space for existing entries to occupy.
        /// This defines how and what entries will get moved to occupy that space. If we're not removing from the visible window, this is ignored,
        /// and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        /// <exception cref="ArgumentException"> Thrown when trying to remove an invalid index </exception>
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
            if (_recycledEntries.Entries.TryGetValue(index, out RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry))
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
        /// </summary>
        /// <param name="removeAtKey"> The key of the entry to remove. </param>
        /// <param name="fixEntries">
        /// If we're removing from the visible window of entries, then we'll be creating some extra space for existing entries to occupy.
        /// This defines how and what entries will get moved to occupy that space. If we're not removing from the visible window, this is ignored,
        /// and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        public void RemoveAtKey(TKeyEntryData removeAtKey, FixEntries fixEntries = FixEntries.Below)
        {
            RemoveAtIndex(_entryKeyToCurrentIndex[removeAtKey], fixEntries);
        }

        /// <summary>
        /// Removes elements at the given index. Existing entries' indices will be shifted like a list removal.
        /// </summary>
        /// <param name="index"> The index to start removal at. </param>
        /// <param name="count"> The number of entries to remove. </param>
        /// <param name="fixEntries">
        /// If we're removing from the visible window of entries, then we'll be creating some extra space for existing entries to occupy.
        /// This defines how and what entries will get moved to occupy that space. If we're not removing from the visible window, this is ignored,
        /// and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        public void RemoveRangeAtIndex(int index, int count, FixEntries fixEntries = FixEntries.Below)
        {
            for (int i = index + count - 1; i >= index; i--)
            {
                RemoveAtIndex(i, fixEntries);
            }
        }

        /// <summary>
        /// Removes elements at the index corresponding to the given key. Existing entries' indices will be shifted like a list removal.
        /// </summary>
        /// <param name="removeAtKey"> The key of the entry to start removal at. </param>
        /// <param name="count"> The number of entries to remove. </param>
        /// <param name="fixEntries">
        /// If we're removing from the visible window of entries, then we'll be creating some extra space for existing entries to occupy.
        /// This defines how and what entries will get moved to occupy that space. If we're not removing from the visible window, this is ignored,
        /// and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        public void RemoveRangeAtKey(TKeyEntryData removeAtKey, int count, FixEntries fixEntries = FixEntries.Below)
        {
            RemoveRangeAtIndex(_entryKeyToCurrentIndex[removeAtKey], count, fixEntries);
        }
        
        /// <summary>
        /// Appends entries to the end of the recycler. Appended entries will always preserve the currently visible window of entries.
        /// Similar to an insertion at the end of the list, but more efficient.
        /// </summary>
        /// <param name="dataForEntries"> The data for the entries. </param>
        public void AppendEntries(IEnumerable<TEntryData> dataForEntries)
        {
            if (dataForEntries?.Any() ?? false)
            {
                InsertDataForEntriesAt(_dataForEntries.Count, new List<TEntryData>(dataForEntries));
                RecalculateActiveEntries();
            }
        }

       
        /// <summary>
        /// Prepends entries to the start of the recycler. Prepended entries will always preserve the currently visible window of entries.
        /// Existing entries' indices will be shifted like a list insertion.
        /// </summary>
        /// <param name="dataForEntries"> The data for the entries. </param>
        public void PrependEntries(IEnumerable<TEntryData> dataForEntries)
        {
            if (dataForEntries?.Any() ?? false)
            {
                InsertDataForEntriesAt(0, new List<TEntryData>(dataForEntries.Reverse()));
                RecalculateActiveEntries();
            }
        }

        /// <summary>
        /// Each piece of entry data is referenced by its index.
        /// When we insert/remove entry data, indices possibly shift, and we need to update any data structure that references those indices to also shift.
        /// </summary>
        private void ShiftIndices(int startIndex, int shiftAmount)
        {
            // Shift our active entries
            Dictionary<int, RecyclerScrollRectEntry<TKeyEntryData, TEntryData>> shiftedActiveEntries = new();

            foreach ((int index, RecyclerScrollRectEntry<TKeyEntryData, TEntryData> activeEntry) in _activeEntries)
            {
                int shiftedIndex = index + (index >= startIndex ? shiftAmount : 0);
                if (shiftedIndex != index)
                {
                    activeEntry.SetIndex(shiftedIndex);
                }

                shiftedActiveEntries[shiftedIndex] = activeEntry;
            }
            
            _activeEntries.Clear();
            foreach ((int index, RecyclerScrollRectEntry<TKeyEntryData, TEntryData> shiftedActiveEntry) in shiftedActiveEntries)
            {
                _activeEntries[index] = shiftedActiveEntry;
            }

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

        protected override void LateUpdate()
        {
            // Handles scrolling
            base.LateUpdate();

            // The base ScrollRect has [ExecuteAlways] but the recycler does not work as such
            if (!Application.isPlaying)
            {
                // Ensure our hierarchy with its components are set up properly
                #if UNITY_EDITOR
                InspectorCheckRootEntriesComponents();
                #endif
                
                return;
            }

            // Update what should be in our start or end cache
            RecalculateActiveEntries();

            // We now have the final set of entries in their correct positions for this frame.
            // Give the user the opportunity to query/operate on them knowing they won't shift.
            OnRecyclerUpdated?.Invoke();
        }

        /// <summary>
        /// Determines what entries are visible, which are not, and what entries need to be in the start and end caches.
        /// Creates and recycles entries accordingly.
        /// </summary>
        private void RecalculateActiveEntries()
        {
            // Check which entries are visible, which are not, and what entries need to be in the start/end caches
            UpdateVisibility();
            
            bool didActiveEntriesChange = _activeEntriesWindow.AreActiveEntriesDirty;
            if (didActiveEntriesChange)
            {
                // Determine what entries need to be removed (offscreen and too far away to belong in the caches)
                _toRecycleEntries.Clear();
                foreach ((int index, RecyclerScrollRectEntry<TKeyEntryData, TEntryData> _) in _activeEntries)
                {
                    if (!_activeEntriesWindow.Contains(index))
                    {
                        _toRecycleEntries.AddLast(index);
                    }
                }
                
                // Recycle unneeded entries
                LinkedListNode<int> current = _toRecycleEntries.First;
                while (current != null)
                {
                    _toRecycleEntries.RemoveFirst();
                    RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry = _activeEntries[current.Value];
                    SendToRecycling(_activeEntries[current.Value]);
                    current = _toRecycleEntries.First;
                }

                // Fill the screen and then the start and end caches with entries
                do
                {
                    _activeEntriesWindow.SetActiveEntriesNonDirty();
                    
                    // Determine what entries need to be added to the start cache
                    _newCachedStartEntries.Clear();
                    if (_activeEntriesWindow.StartCacheIndexRange.HasValue)
                    {
                        for (int i = _activeEntriesWindow.StartCacheIndexRange.Value.End; i >= _activeEntriesWindow.StartCacheIndexRange.Value.Start; i--)
                        {
                            if (!_activeEntries.ContainsKey(i))
                            {
                                _newCachedStartEntries.AddLast(i);
                            }
                        }
                    }

                    // Determine what entries need to be added to the end cache
                    _newCachedEndEntries.Clear();
                    if (_activeEntriesWindow.EndCacheIndexRange.HasValue)
                    {
                        for (int i = _activeEntriesWindow.EndCacheIndexRange.Value.Start; i <= _activeEntriesWindow.EndCacheIndexRange.Value.End; i++)
                        {
                            if (!_activeEntries.ContainsKey(i))
                            {
                                _newCachedEndEntries.AddLast(i);
                            }
                        }
                    }
                    
                    // Create new entries in the start cache
                    current = _newCachedStartEntries.First;
                    while (current != null)
                    {
                        _newCachedStartEntries.RemoveFirst();
                        CreateAndAddEntry(current.Value, GetSiblingIndexForEntry(current.Value), Orientation == RecyclerScrollRectOrientation.TopToBottom ? FixEntries.Below : FixEntries.Above);
                        current = _newCachedStartEntries.First;
                    }

                    // Create new entries in the end cache
                    current = _newCachedEndEntries.First;
                    while (current != null)
                    {
                        _newCachedEndEntries.RemoveFirst();
                        CreateAndAddEntry(current.Value, GetSiblingIndexForEntry(current.Value), Orientation == RecyclerScrollRectOrientation.TopToBottom ? FixEntries.Above : FixEntries.Below);
                        current = _newCachedEndEntries.First;
                    }
                
                    // Entries put in the cache might actually become instantly visible on screen if there are not enough entries to fill the screen.
                    // Loop until we fill the screen with entries and have filled the start and end caches.
                    UpdateVisibility();
                } while (_activeEntriesWindow.AreActiveEntriesDirty);
            }
            
            // Update the visible state of the active entries
            if (didActiveEntriesChange)
            {
                _updateStateOfEntries = new LinkedList<int>(_activeEntriesWindow);
                LinkedListNode<int> current = _updateStateOfEntries.First;
                
                while (current != null)
                {
                    _updateStateOfEntries.RemoveFirst();
                    int entryIndex = current.Value;
                    _activeEntries[entryIndex].SetVisibility(_activeEntriesWindow.IsVisible(entryIndex));
                    current = _updateStateOfEntries.First;
                }   
            }

            // Append an endcap if we are near the last entry, or remove it if not.
            // Note that as it belongs at the end of the list, it will not shift any of the active entries.
            UpdateEndcap();

            // Update the visible state of the endcap
            if (_endcap != null && _endcap.gameObject.activeSelf)
            {
                _endcap.SetVisibility(IsInViewport(_endcap.RectTransform, viewport, _rootCanvas.worldCamera));
            }
        }
        
        private int GetSiblingIndexForEntry(int entryIndex)
        {
            foreach (Transform entryTransform in content)
            {
                if (entryTransform.gameObject.name == (entryIndex - 1).ToString())
                {
                    return entryTransform.GetSiblingIndex() + (Orientation == RecyclerScrollRectOrientation.TopToBottom ? 1 : 0);
                }
            }
         
            // Default as zeroth entry
            return Orientation == RecyclerScrollRectOrientation.TopToBottom ? 0 : content.childCount;
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
                _endcap.FetchFromPool();
                
                AddToContent(
                    _endcap.RectTransform,
                    _endcapLayoutBehaviours,
                    Orientation == RecyclerScrollRectOrientation.TopToBottom ? content.childCount : 0,
                    Orientation == RecyclerScrollRectOrientation.TopToBottom ? FixEntries.Above : FixEntries.Below);
            }
        }

        private void RecycleEndcap()
        {
            RemoveFromContent(_endcap.RectTransform, Orientation == RecyclerScrollRectOrientation.TopToBottom ? FixEntries.Above : FixEntries.Below).SetParent(_endcapParent, false);
            _endcap.ReturnToPool();
        }

        private void CreateAndAddEntry(int dataIndex, int siblingIndex, FixEntries fixEntries = FixEntries.Below)
        {
            if (!TryFetchFromRecycling(dataIndex, out RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry))
            {
                entry = Instantiate(_recyclerEntryPrefab, content);
            }
            
            if (!_entryGameObjectLayoutBehaviours.TryGetValue(entry.UidGameObject, out Behaviour[] layoutBehaviors))
            {
                layoutBehaviors = LayoutUtilities.GetLayoutBehaviours(entry.gameObject, true);
                _entryGameObjectLayoutBehaviours[entry.UidGameObject] = layoutBehaviors;
            }
            
            if (entry.Index != dataIndex)
            {
                entry.BindNewData(dataIndex, _dataForEntries[dataIndex]);
            }
            else
            {
                entry.RebindExistingData();
            }

            AddToContent(entry.RectTransform, layoutBehaviors, siblingIndex, fixEntries);
            _activeEntries[dataIndex] = entry;
        }

        /// <summary>
        /// Updates the range of entries that are currently shown.
        /// As we cache the entries just before and just after what is visible, this also affects the range of what is cached.
        /// </summary>
        private void UpdateVisibility()
        {
            foreach (RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry in _activeEntries.Values)
            {
                bool isVisible = IsInViewport(entry.RectTransform, viewport, _rootCanvas.worldCamera);
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
            void EntryIsVisible(RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry)
            {
                int entryIndex = entry.Index;

                if (!_activeEntriesWindow.VisibleIndexRange.HasValue)
                {
                    _activeEntriesWindow.VisibleIndexRange = (entryIndex, entryIndex);
                    _activeEntriesWindow.UpdateCachesFromVisibleRange();
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
                _activeEntriesWindow.UpdateCachesFromVisibleRange();
            }

            // Not visible
            void EntryIsNotVisible(RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry)
            {
                if (!_activeEntriesWindow.VisibleIndexRange.HasValue)
                {
                    return;
                }

                (int Start, int End) newVisibleIndices = _activeEntriesWindow.VisibleIndexRange.Value;
                int entryIndex = entry.Index;
                bool wentOffTop = Vector3.Dot(entry.RectTransform.position - viewport.transform.position, viewport.transform.up) > 0;

                // Note that for any entry to be non-visible there must be at least one other entry pushing it offscreen.
                // This means there's a guaranteed existent entry below/above it, and we can be safe adding +/- 1 to our index window bounds.
                if (Orientation == RecyclerScrollRectOrientation.TopToBottom)
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
                _activeEntriesWindow.UpdateCachesFromVisibleRange();

                // Special case: we have a full screen endcap, meaning no visible indices, but indices in the start cache.
                // The start cache can only get incremented to hold the final index by incrementing the visible range one past the final index.
                // Therefore after updating the caches like normal we reset the visible range back to its proper value of nothing.
                if (newVisibleIndices.Start == _dataForEntries.Count)
                {
                    _activeEntriesWindow.VisibleIndexRange = null;
                }
            }
        }

        /// <summary>
        /// Resets the Recycler to its very beginning elements.
        /// </summary>
        public void ResetToBeginning()
        {
            ScrollToIndexImmediate(0, Orientation == RecyclerScrollRectOrientation.TopToBottom ? ScrollToAlignment.EntryTop : ScrollToAlignment.EntryBottom);
        }

        /// <summary>
        /// Clears the Recycler of all entries and their underlying data.
        /// </summary>
        public void Clear()
        {
            // Stop any active dragging
            StopMovementAndDrag();

            // Stop auto-scrolling to an index
            StopScrollToIndexCoroutine();
            
            // Clear the data for the entries
            _dataForEntries.Clear();

            // Clear the keys for all the data
            _entryKeyToCurrentIndex.Clear();

            // Recycle all the entries
            foreach (RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry in _activeEntries.Values.ToList())
            {
                SendToRecycling(entry);
            }

            // Unbind everything
            foreach (RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry in _recycledEntries.Entries.Values.ToList())
            {
                _recycledEntries.Remove(entry.Index);
                entry.UnbindIndex();
                _unboundEntries.Enqueue(entry);
            }

            // Recycle the end-cap if it exists
            if (IsEndcapActive)
            {
                RecycleEndcap();
            }

            // Reset our window back to one with no entries
            _activeEntriesWindow.Reset();

            // Reset our pivot to whatever its initial value was
            content.pivot = _initPivot;
        }

        private void StopMovementAndDrag()
        {
            OnEndDrag(new PointerEventData(EventSystem.current));
            StopMovement();
        }

        private void SendToRecycling(RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry, FixEntries fixEntries = FixEntries.Below)
        {
            // Handle the GameObject
            RectTransform entryTransform = entry.RectTransform;
            RemoveFromContent(entryTransform, fixEntries);
            entryTransform.SetParent(_poolParent, false);

            // Mark the entry for re-use
            _recycledEntries.Add(entry);

            // Bookkeeping
            _activeEntries.Remove(entry.Index);
            
            // Cleanup and callback
            entry.Recycle();
        }

        private bool TryFetchFromRecycling(int entryIndex, out RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry)
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
                RecyclerScrollRectEntry<TKeyEntryData, TEntryData> firstEntry = _recycledEntries.GetOldestEntry();
                entry = firstEntry;
                _recycledEntries.Remove(firstEntry.Index);
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
        private void AddToContent(RectTransform child, Behaviour[] layoutBehaviours, int siblingIndex, FixEntries fixEntries = FixEntries.Below)
        {
            // Ensure proper hierarchy
            child.SetParent(content, false);
            child.SetSiblingIndex(siblingIndex);

            // Force expand the width (as we cannot do so through the root VerticalLayoutGroup without also controlling the child size).
            // We assume this is the desired behaviour of most recyclers.
            (child.anchorMin, child.anchorMax) = (Vector2.one * 0.5f, Vector2.one * 0.5f);
            child.sizeDelta = child.sizeDelta.WithX(viewport.rect.width);

            // Calculate the auto-sized height of the child
            if (layoutBehaviours != null && layoutBehaviours.Length > 0)
            {
                SetBehavioursEnabled(layoutBehaviours, true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(child);
                SetBehavioursEnabled(layoutBehaviours, false);
            }

            // Calculate the change in parent size given the child's size
            RecalculateContentSize(fixEntries);
        }

        private RectTransform RemoveFromContent(RectTransform child, FixEntries fixEntries = FixEntries.Below)
        {
            // If the child is not visible then shrink in the direction which keeps it off screen and preserves the currently visible entries
            if (!IsInViewport(child, viewport, _rootCanvas.worldCamera))
            {
                fixEntries = IsAboveViewportCenter(child, viewport) ? FixEntries.Below : FixEntries.Above;
            }

            // Remove the child and recalculate the parent's size
            child.gameObject.SetActive(false);
            RecalculateContentSize(fixEntries);

            return child;
        }

        /// <summary>
        /// Called when a child needs to update its height in the recycler.
        /// </summary>
        private void RecalculateContentChildHeight(RectTransform contentChild, float? newHeight, Behaviour[] layoutBehaviours, FixEntries fixEntries = FixEntries.Below)
        {
            // If the child is not visible then grow in the direction which keeps it off-screen and preserves the currently visible entries
            if (!IsInViewport(contentChild, viewport, _rootCanvas.worldCamera))
            {
                fixEntries = IsAboveViewportCenter(contentChild, viewport) ? FixEntries.Below : FixEntries.Above;
            }

            // Directly set the height of the child
            if (newHeight.HasValue)
            {
                (Vector2 prevAnchorMin, Vector2 prevAnchorMax) = (contentChild.anchorMin, contentChild.anchorMax);
                (contentChild.anchorMin, contentChild.anchorMax) = (Vector2.one * 0.5f, Vector2.one * 0.5f);
                contentChild.sizeDelta = contentChild.sizeDelta.WithY(newHeight.Value);
                (contentChild.anchorMin, contentChild.anchorMax) = (prevAnchorMin, prevAnchorMax);
            }
            // Auto-calculate the height of the child
            else if (layoutBehaviours != null && layoutBehaviours.Length > 0)
            {
                SetBehavioursEnabled(layoutBehaviours, true);
                LayoutRebuilder.ForceRebuildLayoutImmediate(contentChild);
                SetBehavioursEnabled(layoutBehaviours, false);   
            }

            // Calculate the change in parent size given the change in the child's size
            RecalculateContentSize(fixEntries);
        }

        /// <summary>
        /// Called when an entry needs to update its height in the recycler.
        /// This should never need to be called directly, instead using RecyclerScrollRectEntry.RecalculateHeight.
        /// </summary>
        /// <param name="entry"> The entry to update the height of. </param>
        /// <param name="newHeight"> The new height the entry should be set to, null if it should be auto-calculated. </param>
        /// <param name="fixEntries">
        /// If we're updating the size of a visible entry, then we'll either be pushing other entries or creating extra space for other entries to occupy.
        /// This defines how and what entries will get moved. If we're not updating an entry in the visible window, this is ignored, and the parameter will
        /// be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        [UsedByRecyclerContent]
        public void RecalculateEntryHeight(RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry, float? newHeight, FixEntries fixEntries = FixEntries.Below)
        {
            if (_activeEntries.ContainsKey(entry.Index))
            {
                RecalculateContentChildHeight(entry.RectTransform, newHeight, _entryGameObjectLayoutBehaviours[entry.UidGameObject], fixEntries);
                RecalculateActiveEntries();   
            }
        }

        /// <summary>
        /// Called when an endcap needs to update its height in the recycler.
        /// This should never need to be called directly, instead using RecyclerScrollRectEndcap.RecalculateHeight.
        /// </summary>
        /// <param name="newHeight"> The new height of the endcap, null if it is auto-calculated. </param>
        /// <param name="fixEntries">
        /// if we're updating the size of a visible endcap, then we'll either be pushing other entries or creating extra space for other entries to occupy.
        /// This defines how and what entries will get moved. If we're not updating an endcap in the visible window, this is ignored, and the parameter will
        /// be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.
        /// </param>
        [UsedByRecyclerContent]
        public void RecalculateEndcapHeight(float? newHeight, FixEntries? fixEntries = null)
        {
            if (IsEndcapActive)
            {
                RecalculateContentChildHeight(_endcap.RectTransform, newHeight, _endcapLayoutBehaviours, fixEntries ?? (Orientation == RecyclerScrollRectOrientation.TopToBottom ? FixEntries.Above : FixEntries.Below));
                RecalculateActiveEntries();   
            }
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
            
            // Recalculate the content layout and size
            content.SetPivotWithoutMoving(content.pivot.WithY(fixEntries == FixEntries.Below ? 0f : fixEntries == FixEntries.Above ? 1f : 0.5f));
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
            
            bool hasFullContent = content.childCount == DataForEntries.Count + (IsEndcapActive ? 1 : 0);
            
            // When we have < fullscreen worth of content to show, the pivot controls where that content is centered in the viewport.
            // As the pivot moves around to preserve scrolls, reset it to what we started with (not that we can scroll anyway).
            if (!this.IsScrollable() && hasFullContent)
            {
                content.pivot = _initPivot;
                normalizedPosition = normalizedPosition.WithY(1f);
                return;
            }
            
            // Note: visibility has not yet been updated, so we cannot rely on our active window to tell which indices are active or not
            bool hasFirstEntry = HasEntryWithIndex(0);
            bool hasLastEntry = _hasEndcap ? IsEndcapActive : HasEntryWithIndex(DataForEntries.Count - 1);

            // If we're reducing the size of the first or last entries, then we might be creating extra space at the top or bottom of the list
            // that can't be filled by future entries. Shift the list up or down to occupy this space.
            if (hasFirstEntry || hasLastEntry)
            {
                (WorldRect viewportRect, WorldRect contentRect) = (viewport.GetWorldRect(), content.GetWorldRect());
            
                Vector3 viewportUp = viewport.up;
                float viewportHeight = viewportRect.Height;
                float sqrTolerance = Mathf.Pow(viewportHeight * 0.001f, 2);
            
                Vector2 contentTopToViewportTop = Vector3.Project(viewportRect.TopLeftCorner - contentRect.TopLeftCorner, viewportUp);
                Vector2 contentBotToViewportBot = Vector3.Project(viewportRect.BotLeftCorner - contentRect.BotLeftCorner, viewportUp);

                bool shiftViewportUp = Vector3.Dot(contentTopToViewportTop, viewportUp) > 0f &&
                                       contentTopToViewportTop.sqrMagnitude > sqrTolerance;

                bool shiftViewportDown = Vector3.Dot(contentBotToViewportBot, -viewportUp) > 0f &&
                                         contentBotToViewportBot.sqrMagnitude > sqrTolerance;

                // Note: if we need to shift the list both up and down then this implies we have < a full-screen's worth of content, which should have been handled earlier.
                if (shiftViewportUp)
                {
                    content.SetPivotWithoutMoving(content.pivot.WithY(1f));
                    normalizedPosition = normalizedPosition.WithY(1f);
                }
                else if (shiftViewportDown)
                {
                    content.SetPivotWithoutMoving(content.pivot.WithY(0f));
                    normalizedPosition = normalizedPosition.WithY(0f);
                }   
            }

            // Maintain our current scroll, preventing jumps, by moving the anchor equal to the size change
            float contentHeight = content.rect.height;
            if (contentHeight > 0)
            {
                content.SetPivotWithoutMoving(initPivot);
                float diffY = content.anchoredPosition.y - initY;
                content.SetPivotWithoutMoving(content.pivot + Vector2.up * -diffY / contentHeight);   
            }

            bool HasEntryWithIndex(int index)
            {
                return content.Children().Any(c => c.gameObject.name == index.ToString());
            }
        }

        /// <summary>
        /// Scrolls to the entry at a given index. The entry doesn't need to be on screen at the time of the call.
        /// </summary>
        /// <param name="index"> The index of the entry to scroll to. </param>
        /// <param name="scrollToAlignment"> The position within the entry to center on. </param>
        /// <param name="scrollSpeedViewportsPerSecond"> The speed of the scroll. </param>
        /// <param name="onScrollComplete"> Callback invoked once we've successfully scrolled to the entry. </param>
        /// <param name="onScrollCancelled"> Callback invoked when the scroll gets cancelled: either by the user scrolling to something else, or the user pressing down on the recycler. </param>
        /// <exception cref="ArgumentException"> Thrown when attempting to scroll to an invalid index. </exception>
        public void ScrollToIndex(
            int index,
            ScrollToAlignment scrollToAlignment = ScrollToAlignment.EntryMiddle,
            float scrollSpeedViewportsPerSecond = DefaultScrollSpeedViewportsPerSecond,
            Action onScrollComplete = null,
            Action onScrollCancelled = null)
        {
            if (index < 0 || index >= _dataForEntries.Count)
            {
                throw new ArgumentException($"index \"{index}\" must be >= 0 and < the length of data \"{_dataForEntries.Count}\"");
            }

            if (_scrollToIndexCoroutine != null)
            {
                StopScrollToIndexCoroutine();
            }

            if (_isPressed)
            {
                onScrollCancelled?.Invoke();
                return;
            }

            _currScrollingToIndex = index;
            _onScrollCancelled = onScrollCancelled;
            _scrollToIndexCoroutine = StartCoroutine(ScrollToIndexInner(scrollToAlignment, scrollSpeedViewportsPerSecond, onScrollComplete));
        }

        /// <summary>
        /// Scrolls to the entry with a given key. The entry doesn't need to be on screen at the time of the call.
        /// </summary>
        /// <param name="key"> The key of the entry to scroll to. </param>
        /// <param name="scrollToAlignment"> The position within the entry to center on. </param>
        /// <param name="scrollSpeedViewportsPerSecond"> The speed of the scroll. </param>
        /// <param name="onScrollComplete"> Callback invoked once we've successfully scrolled to the entry. </param>
        /// <param name="onScrollCancelled"> Callback invoked when the scroll gets cancelled: either by the user scrolling to something else, or the user pressing down on the recycler. </param>
        public void ScrollToKey(
            TKeyEntryData key,
            ScrollToAlignment scrollToAlignment = ScrollToAlignment.EntryMiddle,
            float scrollSpeedViewportsPerSecond = DefaultScrollSpeedViewportsPerSecond,
            Action onScrollComplete = null,
            Action onScrollCancelled = null)
        {
            ScrollToIndex(_entryKeyToCurrentIndex[key], scrollToAlignment, scrollSpeedViewportsPerSecond, onScrollComplete, onScrollCancelled);
        }

        private IEnumerator ScrollToIndexInner(
            ScrollToAlignment scrollToAlignment, 
            float scrollSpeedViewportsPerSecond,
            Action onScrollComplete)
        {
            // The position within the child the scroll will center on (ex: middle, top edge, bottom edge)
            float normalizedPositionWithinChild = ScrollAlignmentToNormalizedPosition(scrollToAlignment);
            
            float distanceLeftToTravelThisFrame = GetFullDistanceToTravelInThisFrame();
            for (;;)
            {
                int index = _currScrollingToIndex.Value;

                float normalizedScrollDistanceLeftToTravelThisFrame = DistanceToNormalizedScrollDistance(distanceLeftToTravelThisFrame);
                float currNormalizedY = normalizedPosition.y;
                float newNormalizedY = 0f;

                // Scroll through entries until the entry we want is active, then we'll know the exact position to center on
                if (!_activeEntriesWindow.Contains(index))
                {
                    // Scroll toward lesser indices
                    if (index < _activeEntriesWindow.ActiveEntriesRange.Value.Start)
                    {
                        newNormalizedY = Mathf.MoveTowards(currNormalizedY, Orientation == RecyclerScrollRectOrientation.TopToBottom ? 1 : 0, normalizedScrollDistanceLeftToTravelThisFrame);
                    }
                    // Scroll toward greater indices
                    else if (index > _activeEntriesWindow.ActiveEntriesRange.Value.End)
                    {
                        newNormalizedY = Mathf.MoveTowards(currNormalizedY, Orientation == RecyclerScrollRectOrientation.TopToBottom ? 0 : 1, normalizedScrollDistanceLeftToTravelThisFrame);
                    }
                    
                    normalizedPosition = normalizedPosition.WithY(newNormalizedY);
                }
                // Find and scroll to the exact position of the now active entry
                else
                {
                    float entryNormalizedY = this.GetNormalizedVerticalPositionOfChild(_activeEntries[index].RectTransform, normalizedPositionWithinChild);
                    
                    // If we're centered on the position, then we're done scrolling
                    if (this.IsAtNormalizedPosition(normalizedPosition.WithY(entryNormalizedY)))
                    {
                        break;
                    }
                    
                    float prevNormalizedPosY = normalizedPosition.y;
                    newNormalizedY = Mathf.MoveTowards(currNormalizedY, Mathf.Clamp01(entryNormalizedY), normalizedScrollDistanceLeftToTravelThisFrame);
                    normalizedPosition = normalizedPosition.WithY(newNormalizedY);

                    // If we can't scroll anymore (we've hit the very end of the list), then we're done scrolling
                    if (Mathf.Approximately(prevNormalizedPosY, normalizedPosition.y))
                    {
                        break;
                    }
                }
                
                distanceLeftToTravelThisFrame -= NormalizedScrollDistanceToDistance(Mathf.Abs(newNormalizedY - currNormalizedY));
                RecalculateActiveEntries();
                
                // If we have less than 0.1% left of a viewport to travel this frame, we say we've travelled enough this frame
                if (distanceLeftToTravelThisFrame < 0.001f * viewport.rect.height)
                {
                    yield return null;
                    distanceLeftToTravelThisFrame = GetFullDistanceToTravelInThisFrame();
                }
            }

            _currScrollingToIndex = null;
            _scrollToIndexCoroutine = null;
            _onScrollCancelled = null;
            
            onScrollComplete?.Invoke();

            // Returns the distance we'd like to scroll in a single frame
            float GetFullDistanceToTravelInThisFrame()
            {
                return scrollSpeedViewportsPerSecond * viewport.rect.height * Time.deltaTime;
            }

            // Returns the normalized scroll distance corresponding to a certain non-normalized distance.
            float DistanceToNormalizedScrollDistance(float distance)
            {
                // Subtracting the viewport from the content height leaves the available space the viewport can move (the scroll distance)
                return Mathf.InverseLerp(0, content.rect.height - viewport.rect.height, distance);
            }

            // Returns the distance corresponding to scrolling a certain normalized distance
            float NormalizedScrollDistanceToDistance(float normalizedScrollDistance)
            {
                // Subtracting the viewport from the content height leaves the available space the viewport can move (the scroll distance)
                return normalizedScrollDistance * (content.rect.height - viewport.rect.height);
            }
        }
        
        /// <summary>
        /// Immediately scrolls to the entry at a given index. The entry doesn't need to be on screen at the time of the call.
        /// </summary>
        /// <param name="index"> The index of the entry to scroll to. </param>
        /// <param name="scrollToAlignment"> The position within the entry to center on. </param>
        /// <exception cref="ArgumentException"> Thrown when attempting to scroll to an invalid index. </exception>
        public void ScrollToIndexImmediate(int index, ScrollToAlignment scrollToAlignment = ScrollToAlignment.EntryMiddle)
        {
            if (index < 0 || index >= _dataForEntries.Count)
            {
                throw new ArgumentException($"index \"{index}\" must be >= 0 and < the length of data \"{_dataForEntries.Count}\"");
            }
            
            StopMovementAndDrag();
            StopScrollToIndexCoroutine();
            
            // If the entry's already active, then scroll to it
            if (_activeEntries.TryGetValue(index, out RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry))
            {
               ScrollToActiveEntry(entry);
               return;
            }
            
            // Otherwise clear and fill up a new window, centering on the entry
            foreach (RecyclerScrollRectEntry<TKeyEntryData, TEntryData> activeEntry in _activeEntries.Values.ToList())
            {
                SendToRecycling(activeEntry);
            }

            _activeEntriesWindow.VisibleIndexRange = null;
            _activeEntriesWindow.UpdateCachesFromVisibleRange();
            
            CreateAndAddEntry(index, 0);
            
            content.SetPivotWithoutMoving(content.pivot.WithY(0.5f));
            normalizedPosition = normalizedPosition.WithY(0.5f);
            
            RecalculateActiveEntries();
            ScrollToActiveEntry(_activeEntries[index]);

            void ScrollToActiveEntry(RecyclerScrollRectEntry<TKeyEntryData, TEntryData> activeEntry)
            {
                for (;;)
                {
                    float entryNormalizedY = this.GetNormalizedVerticalPositionOfChild(
                        activeEntry.RectTransform,
                        ScrollAlignmentToNormalizedPosition(scrollToAlignment));

                    // If we're already centered on the entry we're done scrolling
                    if (this.IsAtNormalizedPosition(normalizedPosition.WithY(entryNormalizedY)))
                    {
                        return;
                    }
                    
                    float prevNormalizedY = normalizedPosition.y;
                    normalizedPosition = normalizedPosition.WithY(Mathf.Clamp01(entryNormalizedY));
                    
                    // If we can't scroll anymore we're done scrolling
                    if (Mathf.Approximately(prevNormalizedY, normalizedPosition.y))
                    {
                        return;
                    }
                    
                    RecalculateActiveEntries();
                }
            }
        }

        /// <summary>
        /// Immediately scrolls to the entry at the given key. The entry doesn't need to be on screen at the time of the call.
        /// </summary>
        /// <param name="key"> The key of the entry to scroll to. </param>
        /// <param name="scrollToAlignment"> The position within the entry to center on. </param>
        public void ScrollToKeyImmediate(TKeyEntryData key, ScrollToAlignment scrollToAlignment = ScrollToAlignment.EntryMiddle)
        {
            ScrollToIndexImmediate(_entryKeyToCurrentIndex[key], scrollToAlignment);
        }

        /// <summary>
        /// Maps a scroll alignment to its corresponding normalized position within a RectTransform.
        /// </summary>
        private float ScrollAlignmentToNormalizedPosition(ScrollToAlignment scrollToAlignment)
        {
            switch (scrollToAlignment)
            {
                // Top edge
                case ScrollToAlignment.EntryTop:
                    return 1f;
                
                // Bottom edge
                case ScrollToAlignment.EntryBottom:
                    return 0f;
                
                // Center
                case ScrollToAlignment.EntryMiddle:
                    default:
                    return 0.5f;
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
            
            // If we are in the midst of updating what entries are active, ensure we don't operate on the removed data
            _toRecycleEntries.Remove(index);
            _newCachedStartEntries.Remove(index);
            _newCachedEndEntries.Remove(index);
            _updateStateOfEntries.Remove(index);

            // Shift the indices of existing entries that will be affected by the deletion
            ShiftIndices(index + 1, -1);

            // Remove the inserted entry from our key mapping
            _entryKeyToCurrentIndex.Remove(_dataForEntries[index].Key);

            // Actual removal (and modification) of underlying data
            _activeEntriesWindow.Remove(index);
            _dataForEntries.RemoveAt(index);
        }

        /// <summary>
        /// Unity input event for when the user starts holding down the recycler. Stops any ScrollToIndex/Key calls.
        /// </summary>
        /// <param name="eventData"> Data about the pointer down event. </param>
        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            if (_scrollToIndexCoroutine != null)
            {
                StopScrollToIndexCoroutine();
            }
        }
        
        /// <summary>
        /// Unity input event for when the user stops holding down the recycler. Permits new ScrollToIndex/Key calls.
        /// </summary>
        /// <param name="eventData"> Data about the pointer up event. </param>
        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _tracker.Clear();

            if (_setTargetFrameRateTo60)
            {
                Application.targetFrameRate = _initFrameRate;
            }
        }

        /// <summary>
        /// Cancels the current ScrollToIndex/Key call.
        /// </summary>
        public void CancelScrollTo()
        {
            if (_scrollToIndexCoroutine != null)
            {
                StopScrollToIndexCoroutine();
            }
        }

        /// <summary>
        /// Returns the endcap
        /// </summary>
        /// <typeparam name="TEndcap"> The type of the endcap </typeparam>
        /// <returns> The endcap </returns>
        public TEndcap GetEndcap<TEndcap>() where TEndcap : RecyclerScrollRectEndcap<TKeyEntryData, TEntryData>
        {
            return Endcap as TEndcap;
        }
        
        /// <summary>
        /// Returns the active entry with the given index
        /// </summary>
        /// <param name="index"> The index of the entry </param>
        /// <typeparam name="TEntry"> The type of your entry </typeparam>
        /// <exception cref="ArgumentException"> There is no active entry with the given index </exception>
        /// <returns> The active entry with the given index </returns>
        public TEntry GetActiveEntryWithIndex<TEntry>(int index) where TEntry : RecyclerScrollRectEntry<TKeyEntryData, TEntryData>
        {
            if (!_activeEntries.TryGetValue(index, out RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry))
            {
                throw new ArgumentException($"There is no active entry with index: {index}");
            }
            return entry as TEntry;
        }
        
        /// <summary>
        /// Returns the active entry with the given key
        /// </summary>
        /// <param name="key"> The key of the entry </param>
        /// <typeparam name="TEntry"> The type of your entry </typeparam>
        /// <exception cref="ArgumentException"> There is no active entry with the given key </exception>
        /// <returns> The active entry with the given key </returns>
        public TEntry GetActiveEntryWithKey<TEntry>(TKeyEntryData key) where TEntry : RecyclerScrollRectEntry<TKeyEntryData, TEntryData>
        {
            if (!_entryKeyToCurrentIndex.TryGetValue(key, out int index) || !_activeEntries.TryGetValue(index, out RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry))
            {
                throw new ArgumentException($"There is no active entry with key: {key}");
            }
            return entry as TEntry;
        }
        
        /// <summary>
        /// Returns the entry with the given index if it is currently active (cached or visible) in the recycler.
        /// </summary>
        /// <param name="index"> The index of the entry </param>
        /// <param name="activeEntry"> The active entry, or null if it is not currently active </param>
        /// <typeparam name="TEntry"> The type of your entry </typeparam>
        /// <returns> Whether the entry with the given index is currently active in the recycler </returns>
        public bool TryGetActiveEntryWithIndex<TEntry>(int index, out TEntry activeEntry) where TEntry : RecyclerScrollRectEntry<TKeyEntryData, TEntryData>
        {
            activeEntry = null;
            if (!_activeEntries.TryGetValue(index, out RecyclerScrollRectEntry<TKeyEntryData, TEntryData> entry))
            {
                return false;
            }
            
            activeEntry = entry as TEntry;
            return true;
        }

        /// <summary>
        /// Returns the entry with the given key if it is currently active (cached or visible) in the recycler.
        /// </summary>
        /// <param name="key"> The key of the entry </param>
        /// <param name="activeEntry"> The active entry, or null if it is not currently active </param>
        /// <returns> Whether the entry with the given index is currently active in the recycler </returns>
        public bool TryGetActiveEntryWithKey<TEntry>(TKeyEntryData key, out TEntry activeEntry) where TEntry : RecyclerScrollRectEntry<TKeyEntryData, TEntryData>
        {
            return TryGetActiveEntryWithIndex(_entryKeyToCurrentIndex[key], out activeEntry);
        }

        private void StopScrollToIndexCoroutine()
        {
            _currScrollingToIndex = null;

            if (_scrollToIndexCoroutine != null)
            {
                StopCoroutine(_scrollToIndexCoroutine);
                _scrollToIndexCoroutine = null;
                
                _onScrollCancelled?.Invoke();
                _onScrollCancelled = null;
            }
        }

        private void SetContentTracker()
        {
            _tracker.Add(this, content, DrivenTransformProperties.AnchorMin | DrivenTransformProperties.AnchorMax | DrivenTransformProperties.Pivot);
            content.anchorMin = new Vector2(0f, Orientation == RecyclerScrollRectOrientation.TopToBottom ? 1f : 0f);
            content.anchorMax = new Vector2(1f, Orientation == RecyclerScrollRectOrientation.TopToBottom ? 1f: 0f);
            content.pivot = new Vector2(0.5f, Orientation == RecyclerScrollRectOrientation.TopToBottom ? 1f : 0f);
        }

        private static void SetBehavioursEnabled(Behaviour[] behaviours, bool isEnabled)
        {
            Array.ForEach(behaviours, l => l.enabled = isEnabled);
        }
    }
}