using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Helper script for testing a recycler with basic string data. Generates and passes the string data. 
/// </summary>
public class TestStringRecyclerData : MonoBehaviour
{
    [SerializeField]
    private RecyclerScrollRect<StringRecyclerData, string> _recycler = null;

    private const int NumEntries = 20;

    private StringRecyclerData NormalRandomString => 
        new (Enumerable.Range(0, Random.Range(2, 6)).Aggregate(string.Empty, (s, _) => s + Guid.NewGuid()));
    
    private StringRecyclerData LongRandomString => 
        new (Enumerable.Range(0, 25).Aggregate(string.Empty, (s, _) => s + Guid.NewGuid()));

    private IEnumerable<StringRecyclerData> InitEntries => Enumerable.Range(0, NumEntries).Select(_ => NormalRandomString);

    private void Start()
    { 
        _recycler.AppendEntries(InitEntries);
        // _recycler.AppendEntries(new [] { "5f578bcd-6e1f-403e-9861-bb118105c5628f0505d8-a157-4e84-9497-686ebed5d463" });
    }
    
    private void Update()
    {
        // Prepend
        if (Input.GetKeyDown(KeyCode.A))
        {
            _recycler.PrependEntries(new [] { NormalRandomString });
        }
        // Append
        else if (Input.GetKeyDown(KeyCode.D))
        {
            _recycler.AppendEntries(new [] { NormalRandomString });
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            _recycler.content.SetPivotWithoutMoving(new Vector2(0f, 1f));
        }
        // Clear
        else if (Input.GetKeyDown(KeyCode.C))
        {
            _recycler.Clear();
        }
        // Reset to beginning
        else if (Input.GetKeyDown(KeyCode.R))
        {
            _recycler.ResetToBeginning();
        }
        // Insertion
        else if (Input.GetKeyDown(KeyCode.L))
        {
            _recycler.InsertRange(0, new [] { NormalRandomString }, FixEntries.Mid);
        }
        // Removal
        else if (Input.GetKeyDown(KeyCode.K))
        {
            _recycler.RemoveRange(0, 1, FixEntries.Above);
        }
        // Scroll to index
        else if (Input.GetKeyDown(KeyCode.M))
        {
            _recycler.ScrollToIndex(12, ScrollToAlignment.EntryMiddle, isImmediate:true);
        }
    }

    private void OnValidate()
    {
        if (_recycler == null)
        {
            _recycler = GetComponent<RecyclerScrollRect<StringRecyclerData, string>>();
        }
    }
}