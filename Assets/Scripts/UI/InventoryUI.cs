using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryUI : MonoBehaviour
{
    
    public void HandleUpdate(Action onBack)
    {
        if(Input.GetButtonDown("Cancel"))
        {
            onBack?.Invoke();
        }
    }
}
