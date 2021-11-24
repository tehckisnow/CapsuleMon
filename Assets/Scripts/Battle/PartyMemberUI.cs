﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;

    private Color normalColor;

    private Mon _mon;

    private void Awake()
    {
        normalColor = nameText.color;
    }

    public void Init(Mon mon)
    {
        _mon = mon;
        UpdateData();

        _mon.OnHPChanged += UpdateData;
    }

    private void UpdateData()
    {
        nameText.text = _mon.Base.Name;
        levelText.text = "Lvl " + _mon.Level;
        Debug.Log($"{_mon.Name}: {_mon.HP}/{_mon.MaxHp}");
        hpBar.SetHP((float)_mon.HP / (float)_mon.MaxHp);
    }

    public void SetSelected(bool selected)
    {
        if(selected)
        {
            nameText.color = GlobalSettings.i.HighlightedColor;
        }
        else
        {
            nameText.color = GlobalSettings.i.UnhighlightedColor; //! normalColor isn't working for some reason
        }
    }
}
