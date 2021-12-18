using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

public enum CancelAction { ChooseNo, CloseMenu, Disabled }

public class ConfirmationMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] Image messageBox;
    [SerializeField] bool messageBoxVisible;
    [SerializeField] TextMeshProUGUI yesText;
    [SerializeField] TextMeshProUGUI noText;
    [SerializeField] CancelAction cancelAction;

    public event Action OnYes;
    public event Action OnNo;
    //public event Action OnClose;

    private bool yesSelected = true;
    public bool YesSelected => yesSelected;
    public GameState prevState;
    private Color unhighlightedColor;

    private void Awake()
    {
        unhighlightedColor = noText.color;
    }

    public void OpenMenu(string message="", Action yesAction=null, Action noAction=null, CancelAction cancelAction=CancelAction.ChooseNo)
    {
        //state is saved to prevState automatically but must be manually used in actions
        prevState = GameController.Instance.state;
        GameController.Instance.state = GameState.ConfirmationMenu;
        yesSelected = true;
        OnYes = yesAction;
        OnNo = noAction;
        this.cancelAction = cancelAction;
        messageText.text = message;
        if(message == "")
        {
            messageBoxVisible = false;
        }
        else
        {
            messageBoxVisible = true;
        }
        if(!messageBoxVisible)
        {
            messageBox.enabled = false;
            messageText.enabled = false;
        }
        else
        {
            messageBox.enabled = true;
            messageText.enabled = true;
        }
        gameObject.SetActive(true);
        UpdateItemSelection();
    }

    public void CloseMenu()
    {
        gameObject.SetActive(false);
    }

    public void HandleUpdate()
    {
        bool prevSelection = yesSelected;

        if(Input.GetButtonDown("Down") || Input.GetButtonDown("Up"))
        {
            yesSelected = !yesSelected;
        }

        if(prevSelection != yesSelected)
        {
            UpdateItemSelection();
        }

        if(Input.GetButtonDown("Submit"))
        {
            if(yesSelected)
            {
                OnYes?.Invoke();
                CloseMenu();
            }
            else
            {
                OnNo?.Invoke();
                CloseMenu();
            }
        }

        if(Input.GetButtonDown("Cancel"))
        {
            if(cancelAction == CancelAction.ChooseNo)
            {
                yesSelected = false;
                UpdateItemSelection();
                OnNo?.Invoke();
                CloseMenu();
            }
            else if(cancelAction == CancelAction.CloseMenu)
            {
                CloseMenu();
            }
            else if(cancelAction == CancelAction.Disabled)
            {
                return;
            }
        }
    }

    private void UpdateItemSelection()
    {
        if(yesSelected)
        {
            yesText.color = GlobalSettings.i.HighlightedColor;
            noText.color = unhighlightedColor;
        }
        else
        {
            yesText.color = unhighlightedColor;
            noText.color = GlobalSettings.i.HighlightedColor;
        }
    }

    public IEnumerator WaitForChoice()
    {
        yield return new WaitUntil(() => GameController.Instance.state != GameState.ConfirmationMenu);
    }
}
