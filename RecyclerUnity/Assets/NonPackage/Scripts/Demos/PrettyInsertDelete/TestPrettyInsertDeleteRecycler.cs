using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RecyclerScrollRect
{
    /// <summary>
    /// Tests the recycler for animating an entry in on insertion/deletion
    /// </summary>
    public class TestPrettyInsertDeleteRecycler : MonoBehaviour
    {
        [SerializeField]
        private PrettyInsertDeleteRecycler _recycler = null;
        
        private const int InitNumEntries = 30;

        private RecyclerValidityChecker<PrettyInsertDeleteData, string> _validityChecker;

        private void Start()
        {
            _validityChecker = new RecyclerValidityChecker<PrettyInsertDeleteData, string>(_recycler);
            _validityChecker.Bind();
            
            _recycler.AppendEntries(Enumerable.Repeat<object>(null, InitNumEntries).Select(_ => new PrettyInsertDeleteData(false)));
        }

        private void Update()
        {
            int dataLength = _recycler.DataForEntries.Count;
            IRecyclerScrollRectActiveEntriesWindow activeEntriesWindow = _recycler.ActiveEntriesWindow;
            
            // Append entry at bottom
            if (Input.GetKeyDown(KeyCode.A))
            {
                _recycler.InsertAtIndex(dataLength, new PrettyInsertDeleteData(true), FixEntries.Below);
            }
            // Delete entry at bottom
            else if (Input.GetKeyDown(KeyCode.D))
            {
                for (int i = activeEntriesWindow.VisibleIndexRange.Value.End;
                     i >= activeEntriesWindow.VisibleIndexRange.Value.Start;
                     i--)
                {
                    PrettyInsertDeleteEntry entry = (PrettyInsertDeleteEntry) _recycler.ActiveEntries[i];
                    if (!entry.IsDeleteing)
                    {
                        entry.AnimateOutAndDelete();
                        break;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            _validityChecker.Unbind();
        }

        private void OnValidate()
        {
            if (_recycler == null)
            {
                _recycler = GetComponent<PrettyInsertDeleteRecycler>();
            }
        }
    }
}
