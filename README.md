# Intro
A Recycler View for Unity, as a native one is not provided. 
There are many complications transforming the given native ScrollRect into a Recycler
but all of these are addressed (and will be explained in more detail in the future).

The code is currently in a state of being cleaned up and polished.

Features include: 
- Appending
- Prepending
- Insertion
- Deletion
- Pooling 
- Dynamically sized entries (auto-calculation supported)
- Resizing (auto-calculation supported)
- Endcaps
- Scrolling to any index (including those off screen)

The heftiest part of the code (and the two complimentary parts of the Recycler) can be found under: 
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
Appends a range of entries to the end of the existing list of data. Functionally equivalent to insertion - but more efficent - as we know we are tacking things on to the end, not inserting into the middle and pushing things on/off screen unpredictably. 
- `entries:` the data for the entries

### PrependEntries
```
void PrependEntries(IEnumerable<TEntryData> entries)
```
Prepends a range of entries to the beginning of the existing list of data. Functionally equivalent to insertion - but more efficent - as we know we are tacking things on to the beginning, not inserting into the middle and pushing things on/off screen unpredictably. Appending is even more preferrable, as we still need to shift the underlying list containing the data (and prepending will cause the most shifts).
- `entries:` the data forr the entries

### Clear
```
void Clear()
```
Clears the Recycler of all entries and their underlying data. A fresh start.

###
```
void ResetToBeginning()
```
Resets the Recycler to its very beginning elements. Note that this is more efficent than a `ScrollToIndex` call with an index of 0 and `isImmediate = true` (an immediate scroll). The immediate scroll still actually scrolls through all the elements - just in one frame. Here we take advantage of knowing we want to return the very beginning by clearing the Recycler and then recreating it with the same data: this gives us our initial window of entries without all the scrolling.    

## RecyclerScrollRectEntry


## RecyclerScrollRectEndcap
