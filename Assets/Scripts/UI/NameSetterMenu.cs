using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NameSetterMenu : MonoBehaviour
{
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] int maxLength = 15;

    private PlayerController player;

    private void Awake()
    {
        player = PlayerController.Instance;
    }

    private void Start()
    {
        FocusOnInput(); //can't be called in Awake()
    }

    public bool SetName()
    {
        if(ValidateName(nameInputField.text))
        {
            player.Name = nameInputField.text;
            GameController.Instance.UpdateNameDisplay();
            return true;
        }
        else
        {
            nameInputField.text = "";
            FocusOnInput();
            return false;
        }
    }

    private bool ValidateName(string name)
    {
        if(name == "")
        {
            return false;
        }
        else if(name == " ")
        {
            return false;
        }
        else if(name.Contains("\n") || name.Contains("\\"))
        {
            return false;
        }
        else if(name.Length > maxLength)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void FocusOnInput()
    {
        //don't call these in Awake() ; Start() is ok
        nameInputField.Select();
        nameInputField.ActivateInputField();

        //EventSystem.current.SetSelectedGameObject(nameInputField.gameObject, null);
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
        GameController.Instance.state = GameState.FreeRoam;
    }

    public void HandleUpdate()
    {
        if(Input.GetKey(KeyCode.Return))
        {
            if(SetName())
            {
                CloseMenu();
            }
        }
    }
}
