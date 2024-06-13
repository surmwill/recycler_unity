using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class RotateCamera : MonoBehaviour
{
    [SerializeField]
    private Camera _camera = null;

    private static readonly Vector3 RotationsPerSecond = new Vector3(0.5f, 1f, 0.75f); 
    
    private void Update()
    {
        _camera.transform.eulerAngles += RotationsPerSecond * 360 * Time.deltaTime;
    }

    private void OnValidate()
    {
        if (_camera == null)
        {
            _camera = GetComponent<Camera>();
        }
    }
}
