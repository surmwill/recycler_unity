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
- Endcap (an optional distinct entry that comes at the very end of the list)
- Scrolling to any index, on-screen or off-screen
- Immediately jumping to any index, on-screen or off-screen
- Lifecycle methods for entries: when they're bound, recycled, and their visibility changes
- Queryable state of what entries are active on-screen or not; easy retrieval of any one
- "Screen Space - Camera" or "Screen Space - Overlay" canvases supported
- Vertical and horizontal orientations supported
- Only uses native Unity UI elements
- Fully commented and documented
- List of demos for learning and debugging
- Easy scene set up: add a recycler component to a `RectTransform` and serialize an entry prefab in it
- Open source: adapt it to your needs

# Getting Started (In One Page)
```
// 1st class: The data you'd like to display
public class DemoRecyclerData : IRecyclerScrollRectData<string>
{
    public string Key => Guid.NewGuid.ToString();  // Or any unique key

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