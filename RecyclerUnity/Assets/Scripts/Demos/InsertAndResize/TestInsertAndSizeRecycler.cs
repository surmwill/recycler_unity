using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestInsertAndSizeRecycler : MonoBehaviour
{
    [SerializeField]
    private InsertAndResizeRecycler _recycler = null;
    
    private const int InitEntries = 20;
    private const int NumberInsertionEntries = 2;
    
    private void Start()
    {
        _recycler.AppendEntries(Enumerable.Repeat(new InsertAndResizeData(false), InitEntries));
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _recycler.InsertRange(3, Enumerable.Repeat(new InsertAndResizeData(true), NumberInsertionEntries));
        }
    }
}
