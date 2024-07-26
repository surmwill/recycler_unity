using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests clearing and adding entries to a recycler, one-by-one
    /// </summary>
    public class TestStateChangesRecycler : MonoBehaviour
    {
        [SerializeField]
        private EmptyRecyclerScrollRect _recycler = null;
        
        // The time it takes for the entries and endcap to change colors
        public const float CrossFadeTimeSeconds = 1.5f;
        
        // Output state changes for the entry with this index
        public const int DebugPrintStateChangesForEntryIndex = 15;

        private const int InitNumEntries = 50;

        private void Start()
        {
            _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitNumEntries));
        }
    }
}
