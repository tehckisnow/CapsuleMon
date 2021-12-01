using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenNameSetterMenu : MonoBehaviour
{
    public void Open()
    {
        GameController.Instance.StartNameSetterMenu();
    }
}
