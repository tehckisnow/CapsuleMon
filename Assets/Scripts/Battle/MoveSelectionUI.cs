﻿using System;
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
            Debug.Log(currentSelection);
            onSelected?.Invoke(currentSelection);
        }
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
}
