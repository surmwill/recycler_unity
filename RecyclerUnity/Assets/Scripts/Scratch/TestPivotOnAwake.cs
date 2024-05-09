using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPivotOnAwake : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log(((RectTransform) transform).pivot);
    }
}
