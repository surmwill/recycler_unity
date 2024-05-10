using System;
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

    private const int NumEntries = 20;

    private int _numCreated = 0;
    
    private IEnumerable<string> InitEntries => 
        (new []
        {
            "5f578bcd-6e1f-403e-9861-bb118105c5628f0505d8-a157-4e84-9497-686ebed5d463",
            "6e3ad54b-8392-40fd-a4b0-9737315b6cdf9bbcb425-aea2-4dd7-b893-37d451db5f42",
            "34490827-40c9-4bdc-b7e3-735fef3cedb2c071abfc-e1c5-45e3-9648-3429f16ab6fd",
            "34490827-40c9-4bdc-b7e3-735fef3cedb2c071abfc-e1c5-45e3-9648-3429f16ab6fd"
        }).Concat(Enumerable.Range(0, NumEntries).Select(_ => RandomString));
    
    private string RandomString => Enumerable.Range(0, Random.Range(2, 6)).Aggregate(string.Empty, (s, _) => s + Guid.NewGuid());

    private void Start()
    {
        _recycler.AppendEntries(InitEntries);
        // _recycler.AppendEntries(new [] { "5f578bcd-6e1f-403e-9861-bb118105c5628f0505d8-a157-4e84-9497-686ebed5d463" });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log(RandomString);
            _recycler.PrependEntries(new [] { RandomString });
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log(RandomString);
            _recycler.AppendEntries(new [] { RandomString });
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
            _recycler.Insert(3, RandomString, FixEntries.Below);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            _recycler.RemoveAt(3, FixEntries.Below);
        }
    }
}