using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPrintRecyclerValues : MonoBehaviour
{
    private RecyclerScrollRect<string> _recycler;

    private void Awake()
    {
        _recycler = GetComponent<RecyclerScrollRect<string>>();
    }

    private void Update()
    {
        Debug.Log($"({_recycler.normalizedPosition.x}, {_recycler.normalizedPosition.y})");
        Debug.Log(_recycler.IsAtBottom());
    }
}
