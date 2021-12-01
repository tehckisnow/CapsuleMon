using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AutoRun : MonoBehaviour
{
    [SerializeField] UnityEvent action;

    private void Start()
    {
        action?.Invoke();
    }
}