using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenMenuInteraction : MonoBehaviour
{
    [SerializeField] public GameObject menuToOpen;

    private GameObject instance;

    public void OpenMenu()
    {
        GameObject instance = Instantiate(menuToOpen);
    }

    public void CloseMenu()
    {
        Destroy(instance);
    }
}
