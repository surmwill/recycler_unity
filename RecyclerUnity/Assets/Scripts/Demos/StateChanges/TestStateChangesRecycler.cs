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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                _recycler.AppendEntries(new[] { new EmptyRecyclerData() });
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                _recycler.Clear();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                _recycler.ResetToBeginning();
            }
        }
    }
}
