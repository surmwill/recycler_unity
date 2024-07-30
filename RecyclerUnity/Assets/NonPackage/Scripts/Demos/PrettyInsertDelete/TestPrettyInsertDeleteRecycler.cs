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
        private const int NumEntriesInsertedAtMiddle = 4;
        
        private const int NumEntriesDeletedBeforeMiddle = 2;
        private const int NumEntriesDeletedAfterMiddle = 1;

        private RecyclerValidityChecker<PrettyInsertDeleteData, string> _validityChecker;

        private void Start()
        {
            _validityChecker = new RecyclerValidityChecker<PrettyInsertDeleteData, string>(_recycler);
            _validityChecker.Bind();
            
            _recycler.AppendEntries(PrettyInsertDeleteData.GenerateData(InitNumEntries, false));
        }

        private void Update()
        {
            int dataLength = _recycler.DataForEntries.Count;
            (int visibleStartIndex, int visibleEndIndex) = _recycler.ActiveEntriesWindow.VisibleIndexRange.Value;
            int middleEntryIndex = visibleStartIndex + (visibleEndIndex - visibleStartIndex + 1) / 2;
            
            if (Input.GetKey(KeyCode.M))
            {
                // Add entries at middle
                if (Input.GetKeyDown(KeyCode.A))
                {
                    _recycler.InsertRangeAtIndex(middleEntryIndex, PrettyInsertDeleteData.GenerateData(NumEntriesInsertedAtMiddle, true, FixEntries.Mid));  
                }
                // Delete entries at middle
                if (Input.GetKeyDown(KeyCode.D))
                {
                    int startDeleteIndex = middleEntryIndex - NumEntriesDeletedBeforeMiddle;
                    int endDeleteIndex = middleEntryIndex + NumEntriesDeletedAfterMiddle;
                    
                    for (int i = startDeleteIndex; i <= endDeleteIndex; i++)
                    {
                        PrettyInsertDeleteEntry entry = (PrettyInsertDeleteEntry) _recycler.ActiveEntries[i];
                        entry.AnimateOutAndDelete(FixEntries.Mid);
                    }
                }
            }
            // Add entry at bottom
            else if (Input.GetKeyDown(KeyCode.A))
            {
                _recycler.InsertAtIndex(dataLength, new PrettyInsertDeleteData(true, FixEntries.Below));
            }
            // Delete entry at bottom
            else if (Input.GetKeyDown(KeyCode.D))
            {
                for (int i = visibleEndIndex; i >= visibleStartIndex; i--)
                {
                    PrettyInsertDeleteEntry entry = (PrettyInsertDeleteEntry) _recycler.ActiveEntries[i];
                    if (!entry.IsDeleteing)
                    {
                        entry.AnimateOutAndDelete(FixEntries.Below);
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
