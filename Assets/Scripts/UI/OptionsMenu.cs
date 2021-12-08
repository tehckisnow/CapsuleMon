using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using TMPro;

public enum Cancel { Default, Disabled, Custom }

public class OptionsMenu : MonoBehaviour
{
    [SerializeField]private Dictionary<int, Option> options;
    private int selectedItem = 0;
    private GameState prevState;
    private Action CancelAction;

    private void Awake()
    {
        options = new Dictionary<int, Option>();
    }

    public void Open(List<string> text, List<Action> actions, Cancel cancel=Cancel.Default, Action cancelAction=null)
    {
        //set state
        prevState = GameController.Instance.state;
        GameController.Instance.state = GameState.OptionsMenu;

        //ensure list sizes match
        if(text.Count != actions.Count)
        {
            Debug.LogError("Text and action lists do not match in size");
            return;
        }

        //add each option
        for(int i = 0; i < text.Count; i++)
        {
            AddOption(text[i], actions[i]);
        }

        //populate UI list
        PopulateUI();
        UpdateItemSelection();

        //setup cancel action
        switch(cancel)
        {
            case Cancel.Disabled:
                CancelAction = () => {};
                break;
            case Cancel.Custom:
                if(cancelAction != null)
                {
                    CancelAction = cancelAction;
                }
                else
                {
                    //! should this default to default or to disabled if set to custom but the action is left off?
                    CancelAction = DefaultCancelAction;
                }
                break;
            case Cancel.Default:
            default:
                CancelAction = DefaultCancelAction;
                break;
        }
    }

    public void CloseMenu()
    {
        //GameController.Instance.state = prevState;
        GameController.Instance.activeOptionsMenu = null;
        Destroy(gameObject);
    }

    public void AddOption(string text, Action action)
    {
        var option = new Option(text, action);
        options.Add(options.Count, option);
    }

    private void WipeUI()
    {
        //delete all children
        foreach(Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void AddOptionUI(Option option)
    {
        var optionObj = new GameObject("textOption");
        optionObj.AddComponent<TextMeshProUGUI>();
        var optionText = optionObj.GetComponent<TextMeshProUGUI>();
        option.Element = optionText;
        optionText.text = option.Text;
        optionText.color = GlobalSettings.i.UnhighlightedColor;
        optionText.font = GlobalSettings.i.Font;
        var rect = optionObj.GetComponent<RectTransform>();
        //rect.sizeDelta = new Vector2(140, 38);
        optionText.transform.parent = gameObject.transform;
    }

    private void PopulateUI()
    {
        WipeUI();
        int i = 0;
        while(i < options.Count)
        {
            AddOptionUI(options[i]);
            i++;
        }
    }

    public void HandleUpdate()
    {
        int prevSelection = selectedItem;

        if(Input.GetButtonDown("Down"))
        {
            ++selectedItem;
        }
        if(Input.GetButtonDown("Up"))
        {
            --selectedItem;
        }

        selectedItem = Mathf.Clamp(selectedItem, 0, options.Count - 1);

        if(prevSelection != selectedItem)
        {
            UpdateItemSelection();
        }

        if(Input.GetButtonDown("Submit"))
        {
            options[selectedItem].Action?.Invoke();
            CloseMenu();
        }
        if(Input.GetButtonDown("Cancel"))
        {
            CancelAction?.Invoke();
        }
    }

    public void DefaultCancelAction()
    {
        GameController.Instance.state = prevState;
        CloseMenu();
    }

    public void DisabledCancelAction()
    {
        
    }

    public void UpdateItemSelection()
    {
        for(int i = 0; i < options.Count; i++)
        {
            if(i == selectedItem)
            {
                //highlighted
                options[i].Element.color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                //unhighlighted
                options[i].Element.color = GlobalSettings.i.UnhighlightedColor;
            }
        }
    }
}

[System.Serializable]
public class Option
{
    private string text;
    private Action action;

    public string Text => text;
    public Action Action => action;

    private TextMeshProUGUI element;
    public TextMeshProUGUI Element {
        get { return element; }
        set { element = value; }
    }

    public Option(string text, Action action)
    {
        this.text = text;
        this.action = action;
    }
}
