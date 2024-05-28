using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scratch : MonoBehaviour
{
    [SerializeField]
    private RectTransform _rectTransform = null;

    [SerializeField]
    private VerticalLayoutGroup _to = null;

    private void Update()
    {
        RectTransform toTransform = (RectTransform) _to.transform;
        
        if (Input.GetKeyDown(KeyCode.A) && _rectTransform.parent != _to.transform)
        {
            _rectTransform.SetParent(toTransform);
            //_rectTransform.SetSiblingIndex(0);
            toTransform.SetPivotWithoutMoving(new Vector2(0.5f, 0f));
            LayoutRebuilder.ForceRebuildLayoutImmediate(toTransform);
            Canvas.ForceUpdateCanvases();
        }
    }
}
