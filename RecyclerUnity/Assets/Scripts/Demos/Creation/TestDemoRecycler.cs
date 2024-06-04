using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestDemoRecycler : MonoBehaviour
{
    [SerializeField]
    private DemoRecycler _recycler = null;
    
    private static readonly string[] Words =
    { 
        "beam",
        "roasted",
        "average",
        "playground",
        "popcorn",
        "breezy",
        "houses",
        "habitual",
        "irritating",
        "political",
        "ahead",
        "abrasive",
        "cover",
        "fire",
        "misty",
        "amusement",
        "seat",
        "earthquake",
        "sip",
        "announce",
        "orange",
        "deep",
        "tense",
        "credit",
        "flashy",
        "drip",
        "insect",
        "risk",
        "hot",
        "absent",
    };

    private void Start()
    {
        DemoRecyclerData[] entryData = new DemoRecyclerData[Words.Length];
        for (int i = 0; i < Words.Length; i++)
        {
            entryData[i] = new DemoRecyclerData(Words[i], Random.ColorHSV());
        }
        
        _recycler.AppendEntries(entryData);
    }
}
