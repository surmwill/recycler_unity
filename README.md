A Recycler View for Unity as a native one is not provided. 
There are many complications transforming the given native ScrollRect into a Recycler
but all of these are addressed (and will be explained in more detail in the future) here. 
The code is currently in a state of being cleaned up and polished.

Features include: 
appending, prepending, insertion, deletion, pooling, 
dynamically sized entries (auto-calculation supported), resizing (auto-calculation supported), endcaps, and scrolling to any index (including those off screen).

The heftiest part of the code can be found under: RecyclerUnity/Assets/Scripts/Recycler/RecyclerScrollRect.cs