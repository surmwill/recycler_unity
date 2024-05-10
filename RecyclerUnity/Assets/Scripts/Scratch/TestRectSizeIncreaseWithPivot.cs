using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRectSizeIncreaseWithPivot : MonoBehaviour
{
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = (RectTransform) transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _rectTransform.sizeDelta += Vector2.up * 100;
        }
    }
}
