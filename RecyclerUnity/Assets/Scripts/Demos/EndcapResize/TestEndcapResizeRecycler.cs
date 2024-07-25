using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests resizing of the endcap
    /// </summary>
    public class TestEndcapResizeRecycler : MonoBehaviour
    {
        [SerializeField]
        private EmptyRecyclerScrollRect _recycler = null;

        private const int NumEntries = 30;

        private void Start()
        {
            _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(NumEntries));
        }

        private void Update()
        {
            // One additional test resizing the endcap, as it is a small test and doesn't justify belonging on its own
            if (Input.GetKeyDown(KeyCode.A) && _recycler.GetStateOfEndcap() == RecyclerScrollRectContentState.ActiveVisible)
            {
                ((EndcapResizeEndcap) _recycler.Endcap).Resize();
            }
        }
    }
}
