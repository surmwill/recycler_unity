using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateOnAwake : MonoBehaviour
{
    [SerializeField]
    private GameObject _instantiatePrefab = null;

    private void Awake()
    {
        Debug.Log("PARENT AWAKE START");
        Instantiate(_instantiatePrefab, transform);
        Debug.Log("PARENT AWAKE END");
    }
}
