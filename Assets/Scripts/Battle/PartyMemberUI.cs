using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] TextMeshProUGUI messageText;
    [SerializeField] string selectedPrefix = "   ";

    private Dictionary<ConditionID, Color> statusColors;
    private Color normalColor;

    private bool toMove = false;
    public bool ToMove => toMove;

    private Mon _mon;

    private void Awake()
    {
        normalColor = nameText.color;
    }

    public void Init(Mon mon)
    {
        statusColors = GlobalSettings.i.StatusColors;
        
        _mon = mon;
        UpdateData();
        SetMessage("");
        
        _mon.OnHPChanged += UpdateData;
        _mon.OnStatusChanged += SetStatusText;
    }

    private void UpdateData()
    {
        nameText.text = _mon.Name;
        levelText.text = "Lvl " + _mon.Level;
        if(_mon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _mon.Status.Id.ToString().ToUpper();
        }
        hpBar.SetHP((float)_mon.HP / (float)_mon.MaxHp);
    }

    private void SetStatusText()
    {
        if(_mon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = _mon.Status.Id.ToString().ToUpper();
            statusText.color = statusColors[_mon.Status.Id];
        }
    }

    public void SetSelected(bool selected=false)
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

    public void SetMoveAndSelected(bool setToMove=false, bool selected=false)
    {
        string nameValue = "";
        if(ToMove)
        {
            nameValue += selectedPrefix;
        }
        nameValue += _mon.Name;
        nameText.text = nameValue;

        if(setToMove && selected)
        {
            nameText.color = GlobalSettings.i.ReorderColorHighlighted;
        }
        else if(setToMove && !selected)
        {
            nameText.color = GlobalSettings.i.ReorderColor;
        }
        else if(!setToMove && selected)
        {
            nameText.color = GlobalSettings.i.HighlightedColor;
        }
        else
        {
            nameText.color = GlobalSettings.i.UnhighlightedColor;
        }
    }

    public void SetToMove(bool val=false)
    {
        toMove = val;
    }

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
