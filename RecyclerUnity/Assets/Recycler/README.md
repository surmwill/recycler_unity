# Recycler Scroll Rect

A UI tool to efficently display long lists of data. Only the section of the list that can fit on-screen is created/managed. The elements used to render one section of the list are reused (or recyled) to display other sections. 

Therefore, an infinite list of data can be displayed with a small finite list of visual elements.

Condensed documentation can be found below, full documentation is present at: https://github.com/surmwill/recycler_unity.

### Features
- Appending
- Prepending
- Insertion
- Deletion
- Pooling 
- Differently sized entries
- Dynamically sized entries (dimensions that change over time)
- Auto-calculated dimensions with `LayoutGroups` and `ContentSizeFitters`
- Endcap (an optional unique entry that comes at the very end of the list)
- Scrolling to any index, on-screen or off-screen
- Immediately jumping to any index, on-screen or off-screen
- Lifecycle methods for entries: when they're bound, recycled, and their visibility changes
- Queryable state of what entries are active on-screen or not; easy retrieval of any one
- "Screen Space - Camera" or "Screen Space - Overlay" canvases supported
- Vertical and horizontal orientations supported
- Only uses native Unity UI elements (uGUI)
- Fully commented and documented
- List of demos for learning and debugging
- Easy scene set up: add a recycler component to a `RectTransform` and serialize an entry prefab in it
- Free and open source: adapt it to your needs

# Getting Started (In One Page)
```
// 1st class: The data you'd like to display
public class DemoRecyclerData : IRecyclerScrollRectData<string>
{
    public string Key => Guid.NewGuid().ToString();  // Or any unique key

    // Anything else...
}

// 2nd class: The recycler entry component which will display your data. Make this into a prefab
public class DemoRecyclerEntry : RecyclerScrollRectEntry<string, DemoRecyclerData>
{
    [SerializeField]
    private Text _entryText = null;

    // Takes data and binds the UI to it
    protected override void OnBind(DemoRecyclerData entryData)
    {
        _entryText.text = entryData.Key;
    }
}

// 3rd class: The recycler component which displays the list of recycler entries. Add it to a `RectTransform`; drag and serialize your recycler entry prefab into it.
public class DemoRecycler : RecyclerScrollRect<string, DemoRecyclerData>
{
    // Empty: only supplies generic types
}

// In your desired script, send the recycler data
DemoRecycler recycler = GetComponent<DemoRecycler>();
IEnumerable<DemoRecyclerData> yourData = CreateYourData();
recycler.AppendEntries(yourData);
```

# Nuances

### Entries are default expanded to the Recycler's width (for a vertical recycler) or height (for a horizontal recycler).

Each entry will be expanded to the full width of the vertical recycler, or the full height for a horizontal recycler, regardless of the values you set. Should you want a different width or height, a child transform with the desired width or height can be created.

### Entries control their own auto-size.

If your content is auto-sized, then entries must control their own width and/or height with their own `ContentSizeFitter`. The root `LayoutGroup` of the entries will not do this for you.

### The only `ILayoutElements` and `ILayoutControllers` entries should have present on their roots is `LayoutGroups` and `ContentSizeFitters`.

Except during explicitly defined times all `ILayoutElements` and `ILayoutControllers` will be disabled on an entry's root for performance reasons. 
This includes things such as `Images`, which should go under a child instead. 

`LayoutGroups` and `ContentSizeFitters` can still go on the entry's root as they are needed for auto-size calculations.

### Entries must update their own height (for a vertical recycler) or width (for a horizontal recycler) through the recycler.

If we have a vertical recycler, in order for a height change to be properly reflected in the recycler, the entry must call `RecalculateDimension` to set its new height. 
Similarly, for a horizontal recycler, we would call `RecalculateDimension`, but pass its width instead.

For example, to animate an entry growing using DoTween in a vertical recycler, the below code is used to update the Recycler at each step.

```
DOTween.To(() => RectTransform.sizeDelta.y, newHeight => RecalculateDimension(newHeight), TargetHeight, Time);
```