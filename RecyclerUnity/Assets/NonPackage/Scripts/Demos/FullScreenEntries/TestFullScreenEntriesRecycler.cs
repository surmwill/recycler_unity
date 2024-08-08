using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Entry for testing a recycler with full screen entries and endcap.
    /// </summary>
    public class TestFullScreenEntriesRecycler : TestRecycler<EmptyRecyclerData, string>
    {
        [SerializeField]
        private EmptyRecyclerScrollRect _recycler = null;

        private const int InitNumEntries = 5;

        protected override RecyclerScrollRect<EmptyRecyclerData, string> ValidateRecycler => _recycler;

        protected override string DemoTitle => "Full-screen entries demo.";

        protected override string DemoDescription => "Tests a recycler with full-screen entries and endcap.";

        protected override string[] DemoButtonDescriptions => null;

        protected override void Start()
        {
            base.Start();
            _recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(InitNumEntries));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                //_recycler.InsertAtIndex(_recycler.DataForEntries.Count - 3, new EmptyRecyclerData(), FixEntries.Above);
                _recycler.AppendEntries(new []{ new EmptyRecyclerData(), new EmptyRecyclerData(), new EmptyRecyclerData()});
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                _recycler.RemoveAtIndex(_recycler.DataForEntries.Count - 1, FixEntries.Below);
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
