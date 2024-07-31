# Intro
A UI Recycler View for Unity, as a native one is not provided.

### Why do I need a Recycler?
Lists of things to display often take up more space than what is available on screen (think of endlessly scrolling through emails or text messages).
It makes no sense to waste resources on displaying what can't be seen. If we can only see 20 text messages, why spend time creating the entire 1000 message conversation?

Recyclers address this. Instead of creating the entire list of things, we only create what can be seen, and a few entries just off-screen to smoothly scroll into.
If we can only see 20 text messages on screen at a time, and we keep 2 extra messages cached on each end, waiting just above and below the screen to smoothly scroll into, we'll be managing the lifecycle of 24 things to display instead of the full 1000.
This increases performance and reduces headaches. Once an entry has gone far enough offscreen, it is not thrown away but re-used for the next visible entry we are scrolling in to.

In the feature videos below, on the left hand side (the hierarchy), you will see the current list of entries. The numbers on these entries will change as we scroll through the list.
Each number represents the current piece of data (its index) that an entry is displaying. 
Importantly, the number of entries stays small; even when we're scrolling through a list of 100s of pieces of data we always have < 20 active at any given moment. 
The changing numbers is exactly the process of recycling: re-using an old entry with new data.

### Why choose this Recycler?
Transforming Unity's ScrollRect into a Recycler is already difficult, and other packages can be found implementing a Recycler, but none offer the functionality given here.
**Other Recylers assume:**
- **Entries have static dimensions.**
  <br>(A text message would be unable to resize after creation to show an asynchronously loaded link preview.)
- **Entries all have the same dimensions.**
  <br>(Each text message would be required to take up the same amount of room in the conversation, leaving lots of empty space.)
- **The data stays static:** no inserting or removing entries without recreating the entire list.
  <br>(No inserting or deleting text messages.)
- **No endcaps**: sometimes you want one slightly different entry from the rest at the bottom of the list, as it serves a different purpose.
  <br>(A loading indicator that fetches and appends the next page of text messages from a database.)
- **No scrolling to an entry:** unless you calculate it's (x,y) or normalized scroll position yourself, which is usually unintuitive.
- **No auto-calculated layouts** you must know the size of your entries before creating them.
  <br>(You would manually have to calculate the size of each encompassing chat bubble for every message.) 

Here, **all those cases are covered with the following features:** 
- Appending
- Prepending
- Insertion
- Deletion
- Pooling 
- Dynamically sized entries (auto-calculation supported)
- Resizing (auto-calculation supported)
- Endcaps
- Scrolling to any index (including those off screen)
- State changes (know when an entry is cached, visible, or in the recycling pool)
- Works with "Screen Space - Camera" or "Screen Space - Overlay" canvases ("World Space" is untested)
- Fully tested: 10+ demos isolating and testing individual pieces of functionality, 7 debug checks running every frame ensuring a valid recycler. (Note: all tests run outside of the package as per Unity Asset Store guidlines.) 
- Only uses native Unity UI elements
- Fully commented and documented
- Easy GameObject set up: simply add your recycler component to a RectTransform.

# Feature Videos
### Basic Functionality
![](README_Images/recycler_basic_functionality.gif)

### Insertion and deletion
Adding and deleting from the end.

![](README_Images/recycler_pretty_insert_and_delete_end.gif)

Adding and deleting from the middle.

![](README_Images/recycler_pretty_insert_and_delete_middle.gif)

### Appending/Prepending/Endcap
- Equivalent behaviour is available for prepending.
- Similar to insertion but more efficient.
  
![](README_Images/recycler_appending_and_endcap.gif)

### Scrolling to index
- Here we are scrolling to index 45.
- Includes indices that are not currently active in the recycler.
- Works with dynamically sized entries.
- Center on different part of the entry: middle, top edge, bottom edge.
- An immediate scroll is available.
  
![](README_Images/recycler_scroll_to_index.gif)

### Auto-sized entries
- Here each entry generates a random number of lines of text. The entry is auto-sized to fit the text using a `VerticalLayoutGroup` and a `ContentSizeFitter`.

![](README_Images/recycler_autosize.gif)

### State changes
- Know when an entry is cached, visible, or in the recycling pool.
- Here entries become yellow when visible, blue when in the start cache, and green when in the end cache.

![](README_Images/recycler_state_change.gif)

# Getting Started

You will need 3 things:
1. The data you will pass to the Recycler (a normal C# class).
2. A recycler entry to bind the data to (a prefab).
3. The Recycler itself (a component).

### The Data

Here is some sample data in which we store a word to display and a background color.

```
public class DemoRecyclerData : IRecyclerScrollRectData<string>
{
    // IRecyclerScrollRectData<string> implementation
    public string Key => Word;

    public string Word { get; private set; }
    
    public Color BackgroundColor { get; private set; }
}
```

Each piece of data is required to have a unique key, implemented by the `IRecyclerScrollRectData<TEntryDataKey>` interface. 
As entries get added and removed their indices change. Instead of keeping track of all the shifting yourself, you can reference pieces of data by their unchanging keys.

(Tip: a quick way to generate unique keys without much thought or structure is to generate a `Guid.NewGuid().ToString()` as a key. Keys are passed as a parameter to reference specific pieces of data;
if you do not require such methods, the actual key is not that important apart from needing _something_ that is unique).

### The Recycler Entry

Recycler entries are prefabs that will get bound to your data. To begin, create the prefab. 

To make it operable with the Recycler you must include a `RecyclerScrollRectEntry<TEntryData, TEntryDataKey>` component at the root of the prefab. 
Specifically, as generic classes cannot be components, you must create an of instance of the generic class with your data and its corresponding key as the types `class DemoRecyclerEntry : RecyclerScrollRectEntry<DemoRecyclerData, string>`

Upon creating the class you will be asked to implement one mandatory lifecycle method, with 3 other optional ones:

```
// Mandatory
protected override void OnBindNewData(DemoRecyclerData entryData)
{
    // Called when this entry has been retrieved from the recycling pool and is being bound to new data
}

// Optional
protected override void OnRebindExistingData()
{
    // Called instead of OnBindNewData when this entry is retrieved from the recycling pool and bound.
    // Here, the only difference is the data being bound is the same data that the entry had before (and still currently contains).
    // By default, nothing gets reset when an entry gets sent to recycling; hence we can pick up from the state right where we left off, just before it got recycled.
    // We might, for example, resume a paused async operation here instead of starting it all over again.
}

// Optional
protected override void OnSentToRecycling()
{
    // Called when this entry has been sent back to the recycling pool.   
}

// Optional
protected override void OnStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
{
    // Called when the state of the entry changes
}
```

We will use the passed data in `OnBindNewData` to adjust the appearance of the entry:

```
[SerializeField]
private Text _wordText = null;

[SerializeField]
private Text _indexText = null;

[SerializeField]
private Image _background = null;

protected override void OnBindNewData(DemoRecyclerData entryData)
{
    // Set the word and background color to whatever is passed in the data
    _wordText.text = entryData.Word;
    _background.color = entryData.BackgroundColor;

    // Display the index (note that Index is a property found in the base class)
    _indexText.text = Index.ToString();
}
```

### The Recycler

Similar to creating the entry, we have a base `class RecyclerScrollRect<TEntryData, TKeyEntryData>`, but must create an instance of this generic class (working with our data and its key) to be used as a component.

```
public class DemoRecycler : RecyclerScrollRect<DemoRecyclerData, string>
{
    // Empty, unless the user wishes to add something
}
```

Then create an empty RectTransform with the desired dimensions for the Recycler. Add our `DemoRecycler` component to that RectTransform. 

Two child GameObjects will be created: `Entries` and `Pool`

![](README_Images/creating_recycler_blank.gif)

Serialize our entry prefab in the Recycler component. The pool is now filled up with entries.

![](README_Images/creating_recycler_adding_entries.gif)

Finally, create the actual data and append it to the Recycler.

```
[SerializeField]
private DemoRecycler _recycler = null;

private static readonly string[] Words =
{ 
    "hold", "work", "wore", "days", "meat",
    "hill", "club", "boom", "tone", "grey",
    "bowl", "bell", "kick", "hope", "over",
    "year", "camp", "tell", "main", "lose",
    "earn", "name", "hang", "bear", "heat",
    "trip", "calm", "pace", "home", "bank",
    "cell", "lake", "fall", "fear", "mood",
    "head", "male", "evil", "toll", "base"
};

private void Start()
{
    // Create data containing the words from the array, each with a random background color
    DemoRecyclerData[] entryData = new DemoRecyclerData[Words.Length];
    for (int i = 0; i < Words.Length; i++)
    {
        entryData[i] = new DemoRecyclerData(Words[i], Random.ColorHSV());
    }
    
    _recycler.AppendEntries(entryData);
}
```

### End Result

![](README_Images/creating_recycler_end_result.gif)

### (Optional) Endcap

It might be useful to have a different entry at the end of the list than the others. 
For example, this entry could have a button to fetch the next page of entries, or do so automatically upon becoming visible for a set amount of time.
(Note: while we have no problem displaying a large amount of entries, fetching all the data to make them from a database could be costly $. Hence paging is still a useful concept.) 

The process is similar to creating a normal entry. Construct your prefab, then to make it operable with the Recycler include a `RecyclerScrollRectEndcap<TEntryData, TEntryDataKey>` component at the root, filled in
with your corresponding key and data types. Again, as generic classes cannot be components we must create an instance of the generic class ourselves, and use that: `public class DemoEndcap : RecyclerScrollRectEndcap<DemoRecyclerData, string>`.

We can optionally implement these two lifecycle methods, but they are not needed here.

```
// Optional
public virtual void OnFetchedFromPool()
{
  // Called when the endcap has been fetched from its pool and become active.
  // It is either visible on screen or waiting just offscreen in the cache.        
}

// Optional
public virtual void OnReturnedToPool()
{
  // Called when the endcap has been returned to its pool
}
```

Serialize the endcap prefab in Recyler. A new pool for the endcap will get created:

![](README_Images/creating_recycler_adding_endcap.gif)

And our end result looks like:

![](README_Images/creating_recycler_endcap.gif)

# Documentation

## RecyclerScrollRect

### InsertAtIndex
```
public void InsertAtIndex(int index, TEntryData entryData, FixEntries fixEntries)
```

Inserts an entry at the given index. Existing entries' indices will be shifted like a list insertion.

<ins>Parameters</ins>
- `index:` the index to insert the entry at
- `entryData:` the data representing the entry
- `fixEntries:` if we're inserting into the visible window of entries, then we'll need to make some room by pushing some existing entries aside. This defines how and what entries will get moved. If we're not inserting into the visible window, this is ignored, and the parameter will be overriden with whatever value only pushes other offscreen entries, preserving the view of what's on-screen.

<ins>Exceptions</ins>
- `ArgumentException:` thrown when trying to insert at an invalid index 

### InsertAtKey
```
public void InsertAtKey(TKeyEntryData insertAtKey, TEntryData entryData, FixEntries fixEntries)
```
Inserts an entry at the given key. Existing entries' indices will be shifted like a list insertion.

<ins>Parameters</ins>
- `insertAtKey:` the key to insert the entry at
- `entryData:` the data representing the entry
- `fixEntries:` if we're inserting into the visible window of entries, then we'll need to make some room by pushing some existing entries aside. This defines how and what entries will get moved. If we're not inserting into the visible window, this is ignored, and the parameter will be overriden with whatever value only pushes other offscreen entries, preserving the view of what's on-screen.

### InsertRangeAtIndex
```
public void InsertRangeAtIndex(int index, IEnumerable<TEntryData> dataForEntries, FixEntries fixEntries)
```
Inserts a range of entries at the given index. Existing entries' indices will be shifted like a list insertion.

<ins>Parameters</ins>
- `index:` the index to insert the entries at
- `dataForEntries:` the data for the entries
- `fixEntries:` if we're inserting into the visible window of entries, then we'll need to make some room by pushing some existing entries aside. This defines how and what entries will get moved. If we're not inserting into the visible window, this is ignored, and the parameter will be overriden with whatever value only pushes other offscreen entries, preserving the view of what's on-screen.

### InsertRangeAtKey
```
public void InsertRangeAtKey(TKeyEntryData insertAtKey, IEnumerable<TEntryData> dataForEntries, FixEntries fixEntries)
```
Inserts a range of entries at the given key. Existing entries' indices will be shifted - equivalent behaviour to inserting into a list.

<ins>Parameters</ins>
- `insertAtKey:` the index to insert the entries at
- `dataForEntries:` the data for the entries
- `fixEntries:` if we're inserting into the visible window of entries, then we'll need to make some room by pushing some existing entries aside. This defines how and what entries will get moved. If we're not inserting into the visible window, this is ignored, and the parameter will be overriden with whatever value only pushes other offscreen entries, preserving the view of what's on-screen.

### RemoveAtIndex
```
public void RemoveAtIndex(int index, FixEntries fixEntries)
```
Removes an element at the given index. Existing entries' indices will be shifted like a list removal.

<ins>Parameters</ins>
- `index:` the index of the entry to remove
- `fixEntries:` if we're removing from the visible window of entries, then we'll be creating some extra space for existing entries to occupy. This defines how and what entries will get moved to occupy that space. If we're not removing from the visible window, this is ignored, and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.

<ins>Exceptions</ins>
- `ArgumentException:` thrown when trying to remove an invalid index 

### RemoveAtKey
```
public void RemoveAtKey(TKeyEntryData removeAtKey, FixEntries fixEntries)
```
Removes an entry at the given key. Existing entries' indices will be shifted - equivalent behaviour to removing from a list.

<ins>Parameters</ins>
- `removeAtKey:` the key of the entry to remove
- `fixEntries:` if we're removing from the visible window of entries, then we'll be creating some extra space for existing entries to occupy. This defines how and what entries will get moved to occupy that space. If we're not removing from the visible window, this is ignored, and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.

### RemoveRangeAtIndex
```
public void RemoveRangeAtIndex(int index, int count, FixEntries fixEntries)
```
Removes a range of entries starting from the given index. Existing entries' indices will be shifted like a list removal.

<ins>Parameters</ins>
- `index:` the index to start removal at
- `count:` the number of entries to remove
- `fixEntries:` if we're removing from the visible window of entries, then we'll be creating some extra space for existing entries to occupy. This defines how and what entries will get moved to occupy that space. If we're not removing from the visible window, this is ignored, and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.

### RemoveRangeAtKey
```
public void RemoveRangeAtKey(TKeyEntryData removeAtKey, int count, FixEntries fixEntries)
```
Removes a range of entries starting from the given key. Existing entries' indices will be shifted like a list removal.

<ins>Parameters</ins>
- `removeAtKey:` the key of the entry to start removal at
- `count:` the number of entries to remove
- `fixEntries:` if we are removing from the visible window of entries, then extra room will be created, pulling entries in. This defines how and what entries will move to fill up the new space.

### AppendEntries
```
public void AppendEntries(IEnumerable<TEntryData> dataForEntries)
```
Appends entries to the end of the recycler. Appended entries will always preserve the currently visible window of entries.
Similar to an insertion at the end of the list, but more efficient.

<ins>Parameters</ins>
- `dataForEntries:` the data for the entries

### PrependEntries
```
public void PrependEntries(IEnumerable<TEntryData> entries)
```
Prepends entries to the start of the recycler. Prepended entries will always preserve the currently visible window of entries.
Existing entries' indices will be shifted like a list insertion.

<ins>Parameters</ins>
- `dataForEntries:` the data for the entries

### Clear
```
public void Clear()
```
Clears the Recycler of all entries and their underlying data.

### ResetToBeginning
```
public void ResetToBeginning()
```
Resets the Recycler to its very beginning elements.

### ScrollToIndex
```
public void ScrollToIndex(int index, ScrollToAlignment scrollToAlignment, Action onScrollComplete, float scrollSpeedViewportsPerSecond, bool isImmediate)
```
Scrolls to an entry at a given index. The entry doesn't need to be on-screen at the time of the call.

<ins>Parameters</ins>
- `index:` the index of the entry to scroll to
- `scrollToAlignment:` the position within the entry we want to center on
- `onScrollComplete:` callback invoked once we've successfully scrolled to the entry
- `scrollSpeedViewportsPerSecond:` the speed of the scroll
- `isImmediate:` whether the scroll should complete immediately. (Warning: large jumps can be inefficient)

<ins>Exceptions</ins>
- `ArgumentException:` thrown when attempting to scroll to an invalid index

### ScrollToKey
```
public void ScrollToKey(TKeyEntryData key, ScrollToAlignment scrollToAlignment, Action onScrollComplete, float scrollSpeedViewportsPerSecond, bool isImmediate)
```
Scrolls to an entry with the given key. The entry doesn't need to be on-screen at the time of the call.

<ins>Parameters</ins>
- `key:` the key of the entry to scroll to
- `scrollToAlignment:` the position within the entry we want to center on
- `onScrollComplete:` callback invoked once we've successfully scrolled to the entry
- `scrollSpeedViewportsPerSecond:` the speed of the scroll
- `isImmediate:` whether the scroll should complete immediately. (Warning: large jumps can be inefficient)

### CancelScrollTo
```
public void CancelScrollTo()
```
Cancels the current `ScrollToIndex/Key` call.

### GetCurrentIndexForKey
```
public int GetCurrentIndexForKey(TKeyEntryData key)
```
Returns the current index of the entry with a given key.

<ins>Parameters</ins>
- `key:` the key of the entry to get the current index of

### GetKeyForCurrentIndex
```
public TKeyEntryData GetKeyForCurrentIndex(int index)
```
Returns the key of the entry at the give index.

<ins>Parameters</ins>
- `index:` the index of entry to get the key of

### GetStateOfEntryWithIndex
```
public RecyclerScrollRectContentState GetStateOfEntryWithIndex(int index)
```
Returns the state of the entry with a given index.

<ins>Parameters</ins>
- `index:` the index of the entry to check the state of

### GetStateOfEntryWithKey
```
public RecyclerScrollRectContentState GetStateOfEntryWithCurrentIndex(TKeyEntryData key)
```
Returns the state of the entry with the given key.

<ins>Parameters</ins>
- `key:` the key of the entry to check the state of

### GetStateOfEndcap
```
public RecyclerScrollRectContentState GetStateOfEndcap()
```
Returns the state of the endcap.

### RecalculateEntrySize
```
public void RecalculateEntrySize(RecyclerScrollRectEntry<TEntryData, TKeyEntryData> entry, FixEntries fixEntries)
```
Called when an entry updates its dimensions and needs to alert the recycler of its new size. This should never need to be called directly, instead using `RecyclerScrollRectEntry.RecalculateDimensions`.
Note that this triggers a layout rebuild of the entry, incorporating any changes in its auto-calculated size.

<ins>Parameters</ins>
- `entry:` the entry with an updated dimensions
- `fixEntries:` if we're updating the size of a visible entry, then we'll either be pushing other entries or creating extra space for other entries to occupy. This defines how and what entries will get moved. If we're not updating an entry in the visible window, this is ignored, and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.

### RecalculateEndcapSize
```
public void RecalculateEndcapSize(FixEntries fixEntries)
```
Called when an endcap has updates its dimensions and needs to alert the recycler of its new size. This should never need to be called directly, instead using `RecyclerScrollRectEndcap.RecalculateDimensions`.
Note that this triggers a layout rebuild of the endcap, incorporating any changes in its auto-calculated size.

<ins>Parameters</ins>
- `fixEntries:` if we're updating the size of a visible endcap, then we'll either be pushing other entries or creating extra space for other entries to occupy. This defines how and what entries will get moved. If we're not updating an endcap in the visible window, this is ignored, and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.

### OnPointerDown
```
public void OnPointerDown(PointerEventData eventData)
```
Unity input event for when the user taps on the recycler. Stops any `ScrollToIndex/Key` calls.

### DataForEntries
```
public IReadOnlyList<TEntryData> DataForEntries { get; }
```

Returns the list of data being bound to the entries.

### ActiveEntries
```
public IReadOnlyDictionary<int, RecyclerScrollRectEntry<TEntryData, TKeyEntryData>> ActiveEntries { get; }
```

Returns the currently active entries: visible and cached. The key is their index.

### ActiveEntriesWindow
```
public IRecyclerScrollRectActiveEntriesWindow ActiveEntriesWindow { get; }
```

Returns information about the current index ranges of active entries.

### Endcap
```
public RecyclerScrollRectEndcap<TEntryData, TKeyEntryData> Endcap { get; }
```

Returns a reference to the endcap - if it exists.

### AppendTo
```
public RecyclerPosition AppendTo { get; }
```

The position in the recycler that appended entries are added to.

### OnRecyclerUpdated
```
public event Action OnRecyclerUpdated
```

Invoked at the end of LateUpdate once scrolling has been handled. 
Here, the current viewport of entries is not expected to change for the remainder of the frame except through manual user calls. 
The state of the entries can be queried here without worry of them changing.

## RecyclerScrollRectEntry

### UnboundIndex
```
public const int UnboundIndex;
```

The index corresponding to an unbound entry.

### Index
```
public int Index { get; }
```

The current index of the entry (note that indices can shift as things are added and removed).

### Data
```
public TEntryData Data { get; }
```

The data this entry is currently bound to.

### State
```
public RecyclerScrollRectContentState State { get; }
```

The current state of the entry, valid post-binding/rebinding.

### Recyler
```
public RecyclerScrollRect<TEntryData, TKeyEntryData> Recycler { get; }
```

The recycler this entry is a part of.

### RectTransform
```
public RectTransform { get; } 
```

The entry's RectTransform.

### UIDGameObject
```
public int UidGameObject { get; private set; }
```

A unique id representing the GameObject this entry lives on.

### OnBindNewData
```
protected abstract void OnBindNewData(TEntryData entryData)
```

Lifecycle method called when the entry becomes active and bound to a new piece of data.

<ins>Parameters</ins>
- `entryData:` the data the entry is being bound to.

### OnRebindExistingData
```
protected virtual void OnRebindExistingData()
```

Lifecycle method called instead of `OnBindNewData` when the data to be bound to is the same data that's already bound. 
(Note that entries maintain their state when recycled, only losing it when being bound to new data).

### OnSentToRecycling
```
protected virtual void OnSentToRecyling()
```

Lifecycle method called when the entry gets sent back to the recycling pool.

### OnStateChanged
```
protected virtual void OnStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
```

Lifecycle method called when the state of the entry changes.
Note that if entry's previous state was in the pool, the new state is the initial state of the entry post-binding/rebinding.

<ins>Parameters</ins>
- `prevState:` the previous state of the entry
- `newState:` the current state of the entry

### RecalculateDimensions
```
protected void RecalculateDimensions(FixEntries fixEntries)
```

Called when an entry updates its dimensions and needs to alert the recycler of its new size.
Note that this triggers a layout rebuild of the entry, incorporating any changes in its auto-calculated size.

<ins>Parameters</ins>
- `fixEntries:` if we're updating the size of a visible entry, then we'll either be pushing other entries or creating extra space for other entries to occupy.
This defines how and what entries will get moved. If we're not updating an entry in the visible window, this is ignored, and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.

### Other

Below are public functions called on the entries by the recycler to manage their state and lifecycle. **They are intended for the user to call.** They are listed here for the sake of completeness.

#### BindNewData
`public void BindNewData(int index, TEntryData entryData)`

Called by the recycler to bind the entry to a new set of data.

#### RebindExisingData
`public void RebindExistingData()`

Called by the recycler to rebind the entry to its currently bound data.

#### OnRecycled
`public void OnRecycled()`

Called by the recycler when the entry gets recycled.

#### UnbindIndex
`public void UnbindIndex()`

Called by the recycler to reset the entry to its default unbound index.

#### SetIndex
`public void SetIndex(int index)`

Called by the recycler to set the the entry's index.

#### SetState
`public void SetState(RecyclerScrollRectContentState newState)`

Called by the recycler to set the current state of the entry.

## RecyclerScrollRectEndcap

### Recyler
```
public RecyclerScrollRect<TEntryData, TKeyEntryData> Recycler { get; }
```

The Recycler this endcap is a part of.

### State
```
public RecyclerScrollRectContentState State { get; }
```

The current state of the endcap, valid post-fetching from the pool.

### RectTransform
```
public RectTransform { get; } 
```

The endcap's RectTransform.

### OnFetchedFromPool
```
public virtual void OnFetchedFromPool()
```

Lifecycle method called by the recycler when the endcap becomes active, being fetched from its pool.

### OnSentToPool
```
public virtual void OnReturnedToPool()
```

Lifecycle method called by the recycler when the end-cap gets returned to its pool.

### OnStateChanged
```
protected virtual void OnStateChanged(RecyclerScrollRectContentState prevState, RecyclerScrollRectContentState newState)
```

Lifecycle method called when the state of the endcap changes.

<ins>Parameters</ins>
- `prevState:` the previous state the endcap was in.
- `newState:` the new state the endcap is currently in.

### RecalculateDimensions
```
protected void RecalculateDimensions(FixEntries? fixEntries)
```

Called when the endcap updates its dimensions and needs to alert the recycler of its new size.
Note that this triggers a layout rebuild of the endcap, incorporating any changes in its auto-calculated size.

<ins>Parameters</ins>
- `fixEntries:` if we're updating the size of a visible endcap, then we'll either be pushing other entries or creating extra space for other entries to occupy.
This defines how and what entries will get moved. If we're not updating an endcap in the visible window, this is ignored, and the parameter will be overriden with whatever value only moves other offscreen entries, preserving the view of what's on-screen.

### Other (SetState)
```
public void SetState(RecyclerScrollRectContentState newState)
```

This method is intended to be called by the recycler **and not the user** to update the endcap's state. It is listed here for the sake of completeness.

## IRecyclerScrollRectActiveEntriesWindow

Interface for the user to query the various index ranges of active entries in the recycler. 
Can be combined with the `RecyclerScrollRect.ActiveEntries` dictionary.

### Exists
```
public bool Exists { get; }
```

Returns true if the window exists, that is, we have some underlying recycler data to have a window over in the first place.

### VisibleIndexRange
```
public (int Start, int End)? VisibleIndexRange { get; }
```

The range of entry indices that are visible. Null if the range is empty.

### StartCacheIndexRange
```
public (int Start, int End)? StartCacheIndexRange { get; }
```

The range of entry indices contained in the start cache. Null if the range is empty.

### EndCacheIndexRange
```
public (int Start, int End)? EndCacheIndexRange { get; }
```

The range of entry indices contained in the end cache. Null if the range is empty.

### ActiveEntriesRange
```
public (int Start, int End)? ActiveEntriesRange { get; }
```

The range of indices of active entries: both visible and cached. Null if the range is empty.

### PrintRanges
```
public string PrintRanges();
```

Returns information about the current ranges of entry indices.

### GetEnumerator()
```
public IEnumerator<int> GetEnumerator()
```

Returns the indices of all the active entries in increasing order.

## FixEntries

If we're updating the size of a visible entry, then we'll either be pushing other entries or creating extra space for other entries to occupy.
This enum specifies how and what entries will get moved.

<ins>Values</ins>
- `Below:` all entries below the one modified will stay unmoved. 
- `Above:` all entries above the one modified will stay unmoved. 
- `Mid:` all entries above and below the one modified will be moved equally.

## ScrollToAlignment

Enum defining the position within an entry to center on when we scroll to it.

<ins>Values</ins>
- `EntryMiddle:` center on the middle of the entry.
- `EntryBottom:` center on the bottom edge of the entry.
- `EntryTop:` center on the top edge of the entry.

## RecyclerScrollRectContentState

An enum describing the states that recycler entries or the endcap can be in.

<ins>Values</ins>
- `InactiveInPool:` the object is inactive in its pool, waiting to become active.
- `ActiveVisible:` the object is active and visible on-screen.
- `ActiveInStartCache:` the object is active, waiting just offscreen to be scrolled to.
- `ActiveInEndCache:`  the object is active, waiting just offscreen to be scrolled to.

Objects start as `InactiveInPool`, move to a `ActiveInStartCache` or `ActiveInEndCache` as we begin to scoll closer toward them, 
become `ActiveVisible` upon entering the viewport, move to `ActiveInStartCache` or `ActiveInEndCache` as we begin to scroll away from them,
and return to `InActiveInPool` once we have scrolled far enough away.

If the scroll is particularly large, objects can move from `InactiveInPool` to `ActiveVisible` immediately.

## RecyclerPosition

An enum defining two important locations of the recycler list: the top and bottom.

<ins>Values</ins>
- `Top:` the top of the recycler list.
- `Bot:` the bottom of the recycler list.

# Nuances

### Entries set their own auto-size.

If your content is auto-sized, then entries must control their own width and height as a side effect of necessary performance concessions (editor checks will ensure this).

Instead of what is typically done, with having the root (Entries) controlling sizes:

<pre>
Entries
  |- Entry 1 (<strong>VerticalLayoutGroup</strong> with <i>controlChildHeight</i>, <i>controlChildWidth</i> checked)
  |- Entry 2 (<strong>VerticalLayoutGroup</strong> with <i>controlChildHeight</i>, <i>controlChildWidth</i> checked)
  |- Entry 3 (<strong>VerticalLayoutGroup</strong> with <i>controlChildHeight</i>, <i>controlChildWidth</i> checked)
</pre>
  
We need:

<pre>
Entries
  |- Entry 1 (<strong>VerticalLayoutGroup</strong> with <i>controlChildHeight</i>, <i>controlChildWidth</i> checked, and a <strong>ContentSizeFitter</strong>)
  |- Entry 2 (<strong>VerticalLayoutGroup</strong> with <i>controlChildHeight</i>, <i>controlChildWidth</i> checked, and a <strong>ContentSizeFitter</strong>)
  |- Entry 3 (<strong>VerticalLayoutGroup</strong> with <i>controlChildHeight</i>, <i>controlChildWidth</i> checked, and a <strong>ContentSizeFitter</strong>)
</pre>

### Entries are default expanded to the Recycler's width.

The root transform of each entry will be force expanded to the width of the recycler. Should you want a different width, a child transform with the desired width can be created and the root left empty.

### The only `ILayoutElements` and `ILayoutControllers` entries should have present on their roots is `LayoutGroups` and `ContentSizeFitters`.

Except during explicitly defined times all `ILayoutElements` and `ILayoutControllers` will be disabled on an entry's root for performance reasons. 
This includes things such as `Images`, which should go under a child transform instead. 

`LayoutGroups` and `ContentSizeFitters` can still go on the root as they are needed for auto-size calculations.

### The Recycler must be manually informed of an entry's dimension changes.

Every dimension change of an entry, except during binding/rebinding, must be followed by a call [RecalculateDimensions](https://github.com/surmwill/recycler_unity/tree/master/README.md#recalculatedimensions) to alert the Recycler of it.

For example, to animate an entry growing using DoTween, the below code is used to update the Recycler at each step.

```
RectTransform.DOSizeDelta(RectTransform.sizeDelta.WithY(GrowSize), GrowTimeSeconds)
            .OnUpdate(() => RecalculateDimensions());
```

# Repo Organization
The package is self-contained under the directory:
- [RecyclerUnity/Assets/Recycler/](RecyclerUnity/Assets/Recycler/)

Its scripts can be found under:
- [RecyclerUnity/Assets/Recycler/Scripts/](RecyclerUnity/Assets/Recycler/Scripts/)

The two core classes - the Recycler and its entries - can be found under: 
- [RecyclerUnity/Assets/Recycler/Scripts/RecyclerScrollRect.cs](RecyclerUnity/Assets/Recycler/Scripts/RecyclerScrollRect.cs)
- [RecyclerUnity/Assets/Recycler/Scripts/RecyclerScrollRectEntry.cs](RecyclerUnity/Assets/Recycler/Scripts/RecyclerScrollRectEntry.cs) 

Demo Scenes used for isolating and testing behaviour can be found under:
- [RecyclerUnity/Assets/NonPackage/Scenes/Demos/](RecyclerUnity/Assets/NonPackage/Scenes/Demos/)

The code used in the demos can be found under:
- [RecyclerUnity/Assets/NonPackage/Scripts/Demos/](RecyclerUnity/Assets/NonPackage/Scripts/Demos/)
