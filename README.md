A Recycler View for Unity, as a native one is not provided. 
There are many complications transforming the given native ScrollRect into a Recycler
but all of these are addressed (and will be explained in more detail in the future) here. 
The code is currently in a state of being cleaned up and polished.

Features include: 
appending, prepending, insertion, deletion, pooling, 
dynamically sized entries (auto-calculation supported), resizing (auto-calculation supported), endcaps, and scrolling to any index (including those off screen).

The heftiest part of the code can be found under: 
- [RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRect.cs](RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRect.cs)
- [RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRectEntry.cs](RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRectEntry.cs) 

### Basic Functionality Video
![](https://github.com/surmwill/recycler_unity/blob/master/README_Images/recycler_basic_functionality_circles.gif)

### Insertion/Resizing
![](https://github.com/surmwill/recycler_unity/blob/master/README_Images/recycler_insertion_resize.gif)

### Deletion (15, 17, 18)
![](https://github.com/surmwill/recycler_unity/blob/master/README_Images/recycler_deletion.gif)

### Scrolling to index (65)
- Includes indices that are not currently active in the recycler
- Works with dynamically sized entries
  
 ![](https://github.com/surmwill/recycler_unity/blob/master/README_Images/recycler_scroll_to_index.gif)

