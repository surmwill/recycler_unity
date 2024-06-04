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
        "hold",
        "work",
        "wore",
        "days",
        "meat",
        "hill",
        "club",
        "boom",
        "tone",
        "grey",
        "bowl",
        "bell",
        "kick",
        "hope",
        "over",
        "year",
        "camp",
        "tell",
        "main",
        "lose",
        "earn",
        "name",
        "hang",
        "bear",
        "heat",
        "trip",
        "calm",
        "pace",
        "home",
        "bank",
        "cell",
        "lake",
        "fall",
        "fear",
        "mood",
        "head",
        "male",
        "evil",
        "toll",
        "base"
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
