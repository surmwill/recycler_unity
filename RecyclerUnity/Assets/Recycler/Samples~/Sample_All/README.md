This folder contains demos of the recycler for learning purposes. It is not the dependency of anything and can be safely deleted if desired.

# SETUP: 

For animation purposes, these demos require the DOTween package (https://dotween.demigiant.com/) and a reference to the DOTween asmdef. If one does not exist in your project in can be created through the DoTweenUtilityPanel: Tools -> Demigiant -> DoTween Utility Panel, and then clicking on "Create ASMDEF..."

Click the "Setup" button on the "SetupRecyclerDemos" ScriptableObject. This adds the necessary demo scenes to your build settings.

Play the "RecyclerDemo_DemoMenu.unity" scene. This is an all encompassing demo containing a menu which allows you to navigate to and from individual functionalities. Note that each functionality has its own scene and can be explored individually through their folder under ./Scenes/Demos/{SomeDemo}

The prefabs for each demo can be found under ./Scenes/Demos/{SomeDemo} and the scripts for each demo can be founder under ./Scripts/Demos/{SomeDemo}. Each demo should work if you change the recycler's orientation in the scene from vertical to horizontal, or back again. If this is not possible, instead there will be two scenes, one for testing the vertical orientation and one for the horizontal.

Each demo has a toolbar with a question mark explaining what functionality it tests and how to test it. You can also return back to the main demo menu through there.

# ADDITIONAL NOTES FOR TESTING:

In the editor, each of these demos runs a "RecyclerValidityChecker" script alongside the recycler. The validity checker ensures (each frame) things like a proper ordering of entries, no duplicates, and that the entries' actual positions in the scene match their reported state.


