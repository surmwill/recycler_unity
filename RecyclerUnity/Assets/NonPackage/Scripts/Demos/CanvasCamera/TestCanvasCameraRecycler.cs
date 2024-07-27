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
        
        private RecyclerValidityChecker<EmptyRecyclerData, string> _validityChecker;

        private void Start()
        {
            _validityChecker = new RecyclerValidityChecker<EmptyRecyclerData, string>(_recycler);
            _validityChecker.Bind();
            
            _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitNumEntries));
        }

        private void OnDestroy()
        {
            _validityChecker.Unbind();
        }

        private void OnValidate()
        {
            if (_recycler == null)
            {
                _recycler = GetComponent<EmptyRecyclerScrollRect>();
            }
        }
    }
}
