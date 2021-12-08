using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NicknameMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI speciesText;
    [SerializeField] Image sprite;
    [SerializeField] TMP_InputField input;
    
    private GameState prevState;
    private Mon mon;

    public void HandleUpdate()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            StartCoroutine(SetName());
        }
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    //overflow to specify what state to return to after setting nick
    public void Open(Mon mon, GameState gameState)
    {
        Open(mon);
        prevState = gameState;
    }

    public void Open(Mon mon)
    {
        prevState = GameController.Instance.state;
        GameController.Instance.state = GameState.SetMonNick;

        speciesText.text = mon.Base.Name;
        sprite.sprite = mon.Base.FrontSprite;
        this.mon = mon;

        StartCoroutine(SelectInput());
    }

    public IEnumerator SelectInput()
    {
        yield return new WaitForSeconds(0.3f);
        input.Select();
        input.ActivateInputField();
    }

    public IEnumerator SetName()
    {
        var name = input.text;
        if(ValidateName(name))
        {
            mon.Name = input.text;
            yield return DialogManager.Instance.ShowDialogText($"{mon.Base.Name} is now known as {mon.Name}!");
            MonParty.GetPlayerParty().UpdateParty();
            CloseMenu();
        }
        else
        {
            yield return DialogManager.Instance.ShowDialogText($"{name} is not a valid name");
            input.text = "";
            input.Select();
            input.ActivateInputField();
        }
    }

    public void CloseMenu()
    {
        GameController.Instance.state = prevState;
        gameObject.SetActive(false);
    }

    private bool ValidateName(string name)
    {
        if(name == "" || name == " ")
        {
            return false;
        }
        else if(name.Contains("\n") || name.Contains("\\"))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
