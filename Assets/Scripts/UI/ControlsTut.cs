using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsTut : MonoBehaviour
{
    [SerializeField] GameObject controlsTutObj;

    private GameState prevState;

    public void Open()
    {
        controlsTutObj.SetActive(true);
        prevState = GameController.Instance.state;
        GameController.Instance.state = GameState.ControlsTut;
    }

    public void Close()
    {
        //!GameController.Instance.state = prevState;
        GameController.Instance.state = GameState.FreeRoam;
        controlsTutObj.SetActive(false);
    }

    public void HandleUpdate()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            Close();
        }
    }
}
