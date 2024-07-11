using System;

/// <summary>
/// Entry data to test inspector options
/// (ex: increasing/decreasing the pool size and having the corresponding amount of GameObjects)
/// </summary>
public class InspectorData : IRecyclerScrollRectData<string>
{
    public string Key => Guid.NewGuid().ToString();
}
