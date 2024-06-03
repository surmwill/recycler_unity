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

### Basic Functionality Video
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

