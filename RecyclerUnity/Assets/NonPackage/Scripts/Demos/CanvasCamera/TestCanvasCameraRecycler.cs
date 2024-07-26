using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests that our recycler works with a Screen Space - Camera Canvas
    /// </summary>
    public class TestCanvasCameraRecycler : MonoBehaviour
    {
        [SerializeField]
        private EmptyRecyclerScrollRect _recycler = null;

        private const int InitNumEntries = 30;

        private void Start()
        {
            _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitNumEntries));
        }
    }
}
