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
    private RecyclerScrollRect<string> _recycler = null;

    private const int NumEntries = 10;

    private bool _hasAppended = false;
    private bool _hasPrepended = false;

    public static bool stop = false;

    
    private IEnumerable<string> InitEntries => 
        (new []
        {
            "5f578bcd-6e1f-403e-9861-bb118105c5628f0505d8-a157-4e84-9497-686ebed5d463",
            "6e3ad54b-8392-40fd-a4b0-9737315b6cdf9bbcb425-aea2-4dd7-b893-37d451db5f42",
            "34490827-40c9-4bdc-b7e3-735fef3cedb2c071abfc-e1c5-45e3-9648-3429f16ab6fd",
            "34490827-40c9-4bdc-b7e3-735fef3cedb2c071abfc-e1c5-45e3-9648-3429f16ab6fd"
        }).Concat(Enumerable.Range(0, NumEntries).Select(_ => RandomString));
    
    private string RandomString => Enumerable.Range(0, Random.Range(2, 6)).Aggregate(string.Empty, (s, _) => s + Guid.NewGuid());
    
    private string LongString => Enumerable.Range(0, 25).Aggregate(string.Empty, (s, _) => s + Guid.NewGuid());

    private void Start()
    { 
        _recycler.AppendEntries(InitEntries);
        // _recycler.AppendEntries(new [] { "5f578bcd-6e1f-403e-9861-bb118105c5628f0505d8-a157-4e84-9497-686ebed5d463" });
        
        /*
        foreach (string entry in InitEntries)
        {
            if (stop)
            {
                return;
            }
            
            _recycler.AppendEntries(new [] { entry });
        }
        */
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _recycler.PrependEntries(new [] { RandomString + RandomString });

            /*
            _recycler.ScrollToIndex(0, ScrollToAlignment.EntryTop, () =>
            {
                if (!_recycler.IsAtTop())
                {
                    throw new Exception("Should be at top");
                }
            }, 1f, true);
            _hasPrepended = true;
            */
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            _recycler.AppendEntries(new [] { RandomString });
            // _recycler.ScrollToIndex(_recycler.DataForEntries.Count - 1, ScrollToAlignment.EntryTop, null, isImmediate:true);
            
            /*
            _recycler.ScrollToIndex(_recycler.DataForEntries.Count - 1, ScrollToAlignment.EntryBottom, () =>
            {
                if (!_recycler.IsAtBottom())
                {
                    throw new Exception("Should be at bottom");
                }
            }, 1f, true);
            _hasAppended = true;
            */
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            _recycler.content.SetPivotWithoutMoving(new Vector2(0f, 1f));
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            _recycler.Clear();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            _recycler.ResetToBeginning();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            _recycler.InsertRange(0, new [] { RandomString }, FixEntries.Above);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            _recycler.RemoveRange(0, 1, FixEntries.Above);
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            _recycler.ScrollToIndex(12, ScrollToAlignment.EntryMiddle, isImmediate:true);
        }
        
        return;
        if (_hasAppended && !_recycler.IsAtBottom())
        {
            throw new Exception("Should be at bottom");
        }

        if (_hasPrepended && !_recycler.IsAtTop())
        {
            throw new Exception("Should be at top");
        }
    }

    private void OnValidate()
    {
        if (_recycler == null)
        {
            _recycler = GetComponent<RecyclerScrollRect<string>>();
        }
    }
}