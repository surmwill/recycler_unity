using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RecyclerScrollRect
{
    public class TestInsertAndResizeRecycler : MonoBehaviour
    {
        [SerializeField]
        private InsertAndResizeRecycler _recycler = null;

        private const int InitNumEntries = 100;
        private const int InsertionIndex = 15;
        private const int NumInsertionEntries = 2;
        
        private RecyclerValidityChecker<InsertAndResizeData, string> _validityChecker;

        private void Start()
        {
            _validityChecker = new RecyclerValidityChecker<InsertAndResizeData, string>(_recycler);
            _validityChecker.Bind();
            
            _recycler.AppendEntries(CreateDataForEntries(InitNumEntries, false));
        }

        private void OnDestroy()
        {
            _validityChecker.Unbind();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                _recycler.InsertRangeAtIndex(InsertionIndex, CreateDataForEntries(NumInsertionEntries, true));
            }
        }

        private IEnumerable<InsertAndResizeData> CreateDataForEntries(int count, bool shouldGrow)
        {
            return Enumerable.Repeat<object>(null, count).Select(_ => new InsertAndResizeData(shouldGrow));
        }

        private void OnValidate()
        {
            if (_recycler == null)
            {
                _recycler = GetComponent<InsertAndResizeRecycler>();
            }
        }
    }
}
