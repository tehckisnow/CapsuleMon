using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MoveSelectionUI : MonoBehaviour
{
    [SerializeField] List<TextMeshProUGUI> moveTexts;
    [SerializeField] Color highlightedColor;
    
    Color unhighlightedColor;

    Action<int> OnSelected;
    Action OnCancel;

    private int currentSelection = 0;

    private void Awake()
    {
        unhighlightedColor = moveTexts[0].color;
    }

    public void SetMoveData(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for(int i = 0; i < currentMoves.Count; ++i)
        {
            moveTexts[i].text = currentMoves[i].Name;
        }

        moveTexts[currentMoves.Count].text = newMove.Name;
    }

    public void ClearData()
    {
        foreach(TextMeshProUGUI text in moveTexts)
        {
            text.text = "";
            text.color = unhighlightedColor;
            currentSelection = 0;
        }
    }

    public void SetOnSelectionAction(Action<int> onSelected)
    {
        Action<int> UnsubAction = default;
        UnsubAction = Unsub;
        void Unsub(int val)
        {
            OnSelected -= onSelected;
            OnSelected -= UnsubAction;
        };
        OnSelected += onSelected;
        OnSelected += UnsubAction;
    }

    public void SetOnCancelAction(Action onCancel)
    {
        Action UnsubAction = default;
        UnsubAction = Unsub;
        void Unsub()
        {
            OnCancel -= onCancel;
            OnCancel -= UnsubAction;
        }
        OnCancel += onCancel;
        OnCancel += UnsubAction;
    }

    public void HandleMoveSelection()
    {
        if(Input.GetButtonDown("Down"))
        {
            ++currentSelection;
        }
        else if(Input.GetButtonDown("Up"))
        {
            --currentSelection;
        }

        currentSelection = Mathf.Clamp(currentSelection, 0, MonBase.MaxNumberOfMoves);

        UpdateMoveSelection(currentSelection);

        if(Input.GetButtonDown("Submit"))
        {
            OnSelected?.Invoke(currentSelection);
        }

        if(Input.GetButtonDown("Cancel"))
        {
            OnCancel.Invoke();
        }
    }

    //Uses passed in onSelected action
    public void HandleMoveSelection(Action<int> onSelected)
    {
        if(Input.GetButtonDown("Down"))
        {
            ++currentSelection;
        }
        else if(Input.GetButtonDown("Up"))
        {
            --currentSelection;
        }

        currentSelection = Mathf.Clamp(currentSelection, 0, MonBase.MaxNumberOfMoves);

        UpdateMoveSelection(currentSelection);

        if(Input.GetButtonDown("Submit"))
        {
            onSelected?.Invoke(currentSelection);
        }
    }

    public void GenericCancel()
    {
        currentSelection = MonBase.MaxNumberOfMoves;
        OnSelected?.Invoke(currentSelection);
    }

    public void UpdateMoveSelection(int selection)
    {
        for(int i = 0; i < MonBase.MaxNumberOfMoves + 1; i++)
        {
            if(i == selection)
            {
                moveTexts[i].color = highlightedColor;
            }
            else
            {
                moveTexts[i].color = unhighlightedColor;
            }
        }
    }

    public void Close()
    {
        ClearData();
        gameObject.SetActive(false);
    }
}
