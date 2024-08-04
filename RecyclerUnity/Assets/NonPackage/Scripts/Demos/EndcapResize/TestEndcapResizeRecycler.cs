using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests resizing of the endcap
    /// </summary>
    public class TestEndcapResizeRecycler : TestRecycler<EmptyRecyclerData, string>
    {
        [SerializeField]
        private EmptyRecyclerScrollRect _recycler = null;

        private const int NumEntries = 30;

        protected override RecyclerScrollRect<EmptyRecyclerData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Endcap resizing demo";

        protected override string DemoDescription => "Tests resizing of the endcap.";

        protected override string[] DemoButtonDescriptions => new[]
        {
            "1: Grows the endcap if it is visible.",
            "2. Resets the endcap to its original size."
        };

        private EndcapResizeEndcap _endcap;

        protected override void Start()
        {
            base.Start();

            _endcap = (EndcapResizeEndcap) _recycler.Endcap;
            _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(NumEntries));
        }

        private void Update()
        {
            // Grows the endcap
            if ((Input.GetKeyDown(KeyCode.A) || DemoToolbar.GetButtonDown(0)) && _endcap.State == RecyclerScrollRectContentState.ActiveVisible)
            {
                _endcap.Grow();
            }
            // Resets the endcap to its original size
            else if (Input.GetKeyDown(KeyCode.C) || DemoToolbar.GetButtonDown(1))
            {
                _endcap.ResetSizeToNormal();
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
