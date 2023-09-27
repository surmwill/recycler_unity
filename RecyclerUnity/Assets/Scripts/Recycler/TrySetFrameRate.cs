using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tries to set the target frame rate
/// </summary>
public class TrySetFrameRate : MonoBehaviour
{
    [SerializeField]
    private int _targetFrameRate = 60;

    private void Awake()
    {
        Application.targetFrameRate = Mathf.Min(_targetFrameRate, Screen.currentResolution.refreshRate);
    }
}
