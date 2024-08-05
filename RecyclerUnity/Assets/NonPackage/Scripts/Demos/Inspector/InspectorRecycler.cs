using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Recycler to test inspector options.
    /// - Swapping entry prefab and ensuring pool is regenerated.
    /// - Swapping endcap prefab and ensuring endcap is switching.
    /// - Ensuring pool spawns and deletes prefabs as its size is adjusted.
    /// - Ensuring a null entry prefab despawns the pool.
    /// - Ensuring a null endcap prefab despawns the endcap.
    /// </summary>
    public class InspectorRecycler : RecyclerScrollRect<EmptyRecyclerData, string>
    {

    }
}
