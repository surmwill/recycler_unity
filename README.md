# Intro
A Recycler View for Unity, as a native one is not provided. 
There are many complications transforming the given native ScrollRect into a Recycler
but all of these are addressed (and will be explained in more detail in the future) here. 
The code is currently in a state of being cleaned up and polished.

Features include: 
appending, prepending, insertion, deletion, pooling, 
dynamically sized entries (auto-calculation supported), resizing (auto-calculation supported), endcaps, and scrolling to any index (including those off screen).

The heftiest part of the code (and the two complimentary parts of the Recycler) can be found under: 
- [RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRect.cs](RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRect.cs)
- [RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRectEntry.cs](RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRectEntry.cs) 

# Features
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
Specifically, as generic classes cannot be components, you must create an of instance of the generic class with your data as the type: `class DemoRecyclerEntry : RecyclerScrollRectEntry<DemoRecyclerData>`

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
     _wordText.text = entryData.Word;
    _background.color = entryData.BackgroundColor;

    _indexText.text = Index.ToString();    // Note that Index is a property found in the base class
}
```

### The Recycler

Similar to the creating the entry, we have a base `class RecyclerScrollRect<TEntryData>` but must create an instance of this generic class to work with our data, and to be used as a component.

```
public class DemoRecycler : RecyclerScrollRect<DemoRecyclerData>
{
    // Empty, unless the user wishes to add something
}
```
