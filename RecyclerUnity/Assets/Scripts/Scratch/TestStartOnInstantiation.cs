using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStartOnInstantiation : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("AWAKE");
    }
    
    private void Start()
    {
        Debug.Log("START");
    }
}
