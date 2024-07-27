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

        private RecyclerValidityChecker<EmptyRecyclerData, string> _validityChecker;
        
        private void Start()
        {
            _validityChecker = new RecyclerValidityChecker<EmptyRecyclerData, string>(_recycler);
            _validityChecker.Bind();
            
            _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(NumEntries));
        }

        private void OnDestroy()
        {
            _validityChecker.Unbind();
        }

        private void Update()
        {
            // One additional test resizing the endcap, as it is a small test and doesn't justify belonging on its own
            if (Input.GetKeyDown(KeyCode.A) && _recycler.GetStateOfEndcap() == RecyclerScrollRectContentState.ActiveVisible)
            {
                ((EndcapResizeEndcap) _recycler.Endcap).Resize();
            }
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
