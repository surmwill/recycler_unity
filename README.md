# Intro
A Recycler View for Unity, as a native one is not provided.

Lists of things to display often take up more space than what is available on screen (think of endlessly scrolling through emails or text messages).
It makes no sense to waste resources on displaying what can't be seen: if we can only see 20 text messages, why spend time creating the entire 1000 message conversation?

Recyclers address this. Instead of creating the entire list of things, we only create what can be seen, and a few entries just off-screen to smoothly scroll into.
If we can only see 20 text messages on screen at a time, and we keep 2 extra messages cached below and above the screen to smoothly scroll into, we'll be managing the lifecycle of 24 things to display instead of the full 1000.
This increases performance and reduces headaches. Once an entry has gone far enough offscreen, it is not thrown away but re-used for the next visible entry we are scrolling in to.

In the feature videos below, on the left hand side (the hierarchy), you will see the current list of entries. The numbers on these entries will change as we scroll through the list.
Each number represents the current piece of data (its index) that an entry is displaying. 
Importantly, the number of entries stays small; even when we're scrolling through a list of 100s of pieces of data we always have < 20 active at any given moment. 
The changing numbers is exactly the process of recycling: re-using an old entry with new data.

### Why choose this Recycler?
Transforming Unity's ScrollRect into a Recycler is already difficult, and other packages can be found implementing a Recycler, but none offer the functionality given here.
Other Recylers assume:
- entries with static dimensions (a text message would be unable to resize and show an asynchronously loaded preview image)
- entries all with the same dimensions (each text message would be required to take up the same amount of room in the conversation, leaving lots of empty space)
- the list stays static: no inserting or removing entries without recreating the entire list (inserting or deleting messages)
- no endcaps: sometimes you want one slightly different entry from the rest at the bottom of the list as it serves a different purpose (for example, fetching the next page of "normal" data to append to the list)
- no scrolling to an entry: unless you calculate it's (x,y) or normalized scroll position yourself (which is usually unintuitive)
- no auto-calculated layout entries: making entries that need to deal with dynamically sizeed content difficult (for example, an entry with text that changes length based on the localized language) 

Here, all those cases are covered along with the following features: 
- Appending
- Prepending
- Insertion
- Deletion
- Pooling 
- Dynamically sized entries (auto-calculation supported)
- Resizing (auto-calculation supported)
- Endcaps
- Scrolling to any index (including those off screen)

The code is currently in a state of being finalized and documented. The heftiest part of the code (and the two complimentary parts of the Recycler) can be found under: 
- [RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRect.cs](RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRect.cs)
- [RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRectEntry.cs](RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRectEntry.cs) 

# Feature Videos
### Basic Functionality
![](README_Images/recycler_basic_functionality_circles.gif)

### Insertion/Resizing
![](README_Images/recycler_insertion_resize.gif)

### Deletion (15, 17, 18)
![](README_Images/recycler_deletion.gif)

### Appending/Prepending/Endcap
- Equivalent behaviour is available for prepending.
- Similar to insertion (insertion can accomplish the exact same thing), but more efficient as we know we are appending to the ends only and won't be pushing around any currently visible entries.
  
![](README_Images/recycler_appending_and_endcap.gif)

### Scrolling to index (45)
- Includes indices that are not currently active in the recycler.
- Works with dynamically sized entries.
  
 ![](README_Images/recycler_scroll_to_index.gif)

 # Getting Started

 You will need 3 things:
 1. The data you will pass to the Recycler (a normal C# class).
 2. A recycler entry to bind the data to (a prefab).
 3. The Recycler itself (a component).

### The Data

Here is some sample data in which we store a word to display and a background color.
```
public class DemoRecyclerData
{
    public string Word { get; private set; }
    
    public Color BackgroundColor { get; private set; }
}
```

### The Recycler Entry

Recycler entries are prefabs that will get bound to your data. To begin, create the prefab. 

To make it operable with the Recycler you must include a `RecyclerScrollRectEntry<TEntryData>` component. 
Specifically, as generic classes cannot be components, you must create an of instance of the generic class with your data as the type `class DemoRecyclerEntry : RecyclerScrollRectEntry<DemoRecyclerData>`

Upon creating the class you will be asked to implement three different lifecycle methods:

```
protected override void OnBindNewData(DemoRecyclerData entryData)
{
    // Called when this entry is bound to new data
}

protected override void OnRebindExistingData()
{
    // Called when this entry is bound, but with the data it had before (and still currently contains)
}

protected override void OnSentToRecycling()
{
    // Called when this entry has been sent back to the recycling pool   
}
```

We will use the passed data in `OnBindNewData` to adjust the appearance of the entry:

```
[SerializeField]
private TMP_Text _wordText = null;

[SerializeField]
private TMP_Text _indexText = null;

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

Similar to creating the entry, we have a base `class RecyclerScrollRect<TEntryData>` but must create an instance of this generic class to work with our data, and to be used as a component.

```
public class DemoRecycler : RecyclerScrollRect<DemoRecyclerData>
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

# Documentation

## RecyclerScrollRect

### Insert
```
void Insert(int index, TEntryData entryData, FixEntries fixEntries)
```
Inserts an entry at the given index. Existing entries will be shifted - equivalent behaviour to inserting into a list.
- `index:` the index to insert the entry at
- `entryData:` the data representing the entry
- `fixEntries:` if we are inserting into the visible window of entries, then we'll need to make some room by pushing some existing entries aside. This defines how and what entries will get moved.

### InsertRange
```
void InsertRange(int index, IEnumerable<TEntryData> entryData, FixEntries fixEntries)
```
Inserts a range of entries at the given index. Existing entries will be shifted - equivalent behaviour to inserting into a list.
- `index:` the index to insert the entries at
- `entryData:` the data for the entries
- `fixEntries:` if we are inserting into the visible window of entries, then we'll need to make some room by pushing some existing entries aside. This defines how and what entries will get moved.

### RemoveAt
```
void RemoveAt(int index, FixEntries fixEntries)
```
Removes an entry at the given index. Existing entries will be shifted - equivalent behaviour to removing from a list.
- `index:` the index of the entry to remove
- `fixEntries:` if we are removing from the visible window of entries, then extra room will be created, pulling entries in. This defines how and what entries will move to fill up the new space.

### RemoveRange
```
void RemoveRange(int index, int count, FixEntries fixEntries)
```
Removes a range of entries starting from the given index. Existing entries will be shifted - equivalent behavior to removing from a list.
- `index:` the index to start removal at
- `count:` the number of entries to remove
- `fixEntries:` if we are removing from the visible window of entries, then extra room will be created, pulling entries in. This defines how and what entries will move to fill up the new space.

### AppendEntries
```
void AppendEntries(IEnumerable<TEntryData> entries)
```
Appends a range of entries to the end of the existing list of data. 

Functionally equivalent to insertion - but more efficent - as we know we are tacking things on to the end, not inserting into the middle and pushing things on/off screen unpredictably. 

- `entries:` the data for the entries

### PrependEntries
```
void PrependEntries(IEnumerable<TEntryData> entries)
```
Prepends a range of entries to the beginning of the existing list of data. 

Functionally equivalent to insertion - but more efficent - as we know we are tacking things on to the beginning, not inserting into the middle and pushing things on/off screen unpredictably. Appending is even more preferrable, as we still need to shift the underlying list containing the data (and prepending will cause the most shifts).

- `entries:` the data for the entries

### Clear
```
void Clear()
```
Clears the Recycler of all entries and their underlying data. A fresh start.

### ResetToBeginning
```
void ResetToBeginning()
```
Resets the Recycler to its very beginning elements. 

Note that this is more efficent than a `ScrollToIndex` call with an index of 0 and `isImmediate = true` (i.e. an immediate scroll to index 0, our first element). The immediate scroll still actually scrolls through all the elements - just in one frame. Here we take advantage of knowing we want to return the very beginning of the Recycler by clearing it and then recreating it with the same data: this gives us our initial window of entries without all the intermediate scrolling. 

### ScrollToIndex
```
void ScrollToIndex(int index, ScrollToAlignment scrollToAlignment, Action onScrollComplete, float scrollSpeedViewportsPerSecond, bool isImmediate)
```
Scrolls to an entry at a given index. The entry doesn't need to be on screen at the time of the call.

- `index:` the index of the entry to scroll to
- `scrollToAlignment:` the position within the entry we want to center on (ex: the middle, the top edge, the bottom edge)
- `onScrollComplete:` callback invoked once we've successfully scrolled to the entry
- `scrollSpeedViewportsPerSecond:` the speed of the scroll, defined in viewports per second (ex: a value of 1 means we'll scroll past 1 full screen of entries every second)
- `isImmediate:` whether the scroll should complete immediately. Warning: the scroll still occurs - just in one frame - meaning large jumps are costly.

### GetStateOfEntry
```
RecyclerScrollRectEntryState GetStateOfEntry(RecyclerScrollRectEntry<TEntryData> entry)
```

Returns the state of an entry, whether it is: 
1. Visible (active, on-screen),
2. In the start cache (active, waiting just off-screen),
4. In the end cache (active, waiting just off-screen),
5. In the recycling pool and bound (inactive, but was previously active and bound to some data we hope we can reuse)
6. In the recycling pool and unbound (inactive, was never previously active and therefore holds no binding data)

- `entry:` the entry to check the state of

### GetStateOfEndcap
```
RecyclerScrollRectEndcapState GetStateOfEndcap()
```
Returns the state of the endcap, whether it is:
1. Visible (active, on-screen)
2. Cached (active, waiting just off-screen)
3. In the recycling pool (inactive, waiting to be needed)

### RecalculateContentChildSize
```
void RecalculateContentChildSize(RectTransform contentChild, FixEntries fixEntries)
```

This function shouldn't be used unless you know what you're doing. It is public to be used by the entries and endcap to alert the Recycler to size changes. 
Technically any old RectTransform can be inserted into the content and this function called to properly display it, but this is undefined behaviour and risks messing up the bookkeeping of what entries are on and off-screen.

- `contentChild:` a RectTransform in the ScrollRect's content that needs to have its size updated and properly displayed.
- `fixEntries:` as the RectTransform grows or shrinks other entries will get pushed away, or pulled in to the empty space. This defines how and what entries will move.

### OnRecyclerUpdated
```
event Action OnRecyclerUpdated
```

Invoked at the end of LateUpdate once scrolling has been handled, and the current viewport of entries is not expected to change (this frame) except through manual user calls (Insert, Delete, etc...). The state of the entries can be queried here without worry of them changing.

### DataForEntries
```
IReadOnlyList<TEntryData> DataForEntries { get; }
```

Returns the list of data being bound to the entries.

### ActiveEntries
```
IReadOnlyDictionary<int, RecyclerScrollRectEntry<TEntryData>> ActiveEntries { get; }
```

Returns the currently active entries (both visible on-screen and cached just off-screen), which can be looked up by their index. Note that `GetStateOfEntry` can be called on any entry here for a more fine-grained state, and, for example, to decipher which entries are in the cache and which are visible. 

## RecyclerScrollRectEntry


## RecyclerScrollRectEndcap
