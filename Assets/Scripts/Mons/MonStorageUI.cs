using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MonStorageUI : MonoBehaviour
{
    [SerializeField] GameObject monStorageUI;
    [SerializeField] GameObject textHolderObject;
    [SerializeField] TextMeshProUGUI textPrefab;
    [SerializeField] int itemsInViewport = 6;
    [SerializeField] Image upArrow;
    [SerializeField] Image downArrow;
    [Header("Content")]
    [SerializeField] RectTransform textHolderRect;

    private List<TextMeshProUGUI> textOptionList;
    private int currentOption = 0;
    private Color unhighlightedColor;
    private MonStorage monStorage;
    private GameState prevState;


    private void Awake()
    {
        monStorage = MonStorage.Instance;
        textOptionList = new List<TextMeshProUGUI>();
        unhighlightedColor = textPrefab.color;
    }

    public void Open()
    {
        //open UI
        monStorageUI.SetActive(true);
        prevState = GameController.Instance.state;
        GameController.Instance.state = GameState.MonStorage;
        currentOption = 0;
        WipeList();
        PopulateList();
        UpdateItemSelection();
    }

    private void WipeList()
    {
        //clear list
        textOptionList = new List<TextMeshProUGUI>();
        foreach(Transform child in textHolderObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void PopulateList()
    {
        //populate list
        for(int i = 0; i < monStorage.Mons.Count; i++)
        {
            var mon = monStorage.Mons[i];
            var newOption = Instantiate(textPrefab, textHolderObject.transform);
            newOption.GetComponent<TextMeshProUGUI>().text = $"{mon.Name} LV: {mon.Level}";
            textOptionList.Add(newOption);
        }
    }

    private void RefreshList()
    {
        WipeList();
        PopulateList();
    }

    public void HandleUpdate()
    {
        int prevSelection = currentOption;

        if(Input.GetButtonDown("Down"))
        {
            currentOption++;
        }
        if(Input.GetButtonDown("Up"))
        {
            currentOption--;
        }
        
        currentOption = Mathf.Clamp(currentOption, 0, textOptionList.Count - 1);

        if(prevSelection != currentOption)
        {
            UpdateItemSelection();
        }

        if(Input.GetButtonDown("Submit"))
        {
            //! replace this with an option menu (info, take, move, release)
       
            //SelectMon();
            OpenWithdrawConfirmation();
        }

        if(Input.GetButtonDown("Cancel"))
        {
            CloseMonStorageUI();
        }
    }

    private void UpdateItemSelection()
    {
        for(int i = 0; i < textOptionList.Count; i++)
        {
            if(i == currentOption)
            {
                textOptionList[i].color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                textOptionList[i].color = unhighlightedColor;
            }
        }

        HandleScrolling();
    }

    private void HandleScrolling()
    {
        if(textOptionList.Count <= itemsInViewport)
        {
            upArrow.gameObject.SetActive(false);
            downArrow.gameObject.SetActive(false);
            return;
        }
        //calculate scrollPos based on height of textOptionList[0] and
        //don't start scrolling until selecting itemsInViewport/2 (i.e. second half of items on screen)
        float height = textOptionList[0].preferredHeight;
        float scrollPos = Mathf.Clamp(currentOption - itemsInViewport/2, 0, currentOption) * height;
        
        textHolderRect.localPosition = new Vector2(textHolderRect.localPosition.x, scrollPos);

        //show arrows
        bool showUpArrow = currentOption > itemsInViewport / 2;
        bool showDownArrow = currentOption + itemsInViewport / 2 < textOptionList.Count;
        upArrow.gameObject.SetActive(showUpArrow);
        downArrow.gameObject.SetActive(showDownArrow);
    }

    public void OpenWithdrawConfirmation()
    {
        if(currentOption >= 0 && currentOption < monStorage.Mons.Count)
        {
            Mon mon = monStorage.Mons[currentOption];
            string message = $"Do you want to withdraw {mon.Name}?";
            Action yesAction = () =>
            {
                void RevertState()
                {
                    DialogManager.Instance.OnDialogFinished -= RevertState;
                    GameController.Instance.state = GameState.MonStorage;
                }

                SelectMon();
                DialogManager.Instance.OnDialogFinished += RevertState;
                GameController.Instance.state = GameState.Dialog;
            };
            Action noAction = () =>
            {
                GameController.Instance.state = GameController.Instance.confirmationMenu.prevState;
            };
            GameController.Instance.OpenConfirmationMenu(message, yesAction, noAction);
        }
    }

    private void SelectMon()
    {
        
        if(currentOption >= 0 && currentOption < monStorage.Mons.Count)
        {
            monStorage.TakeMon(monStorage.Mons[currentOption]);
            RefreshList();
            UpdateItemSelection();
        }
    }

    public void CloseMonStorageUI()
    {
        GameController.Instance.state = prevState;
        monStorageUI.SetActive(false);
    }
}
