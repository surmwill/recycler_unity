using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestInsertAndResizeRecycler : MonoBehaviour
{
    [SerializeField]
    private InsertAndResizeRecycler _recycler = null;
    
    private const int InitNumEntries = 100;
    private const int NumInsertionEntries = 2;
    
    private void Start()
    {
        _recycler.AppendEntries(CreateDataForEntries(InitNumEntries, false));
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            _recycler.InsertRange(61, CreateDataForEntries(NumInsertionEntries, true));
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            _recycler.ScrollToIndex(65);
        }
    }

    private IEnumerable<InsertAndResizeData> CreateDataForEntries(int count, bool shouldGrow)
    {
        return Enumerable.Repeat<object>(null, count).Select(_ => new InsertAndResizeData(shouldGrow));
    }
}
