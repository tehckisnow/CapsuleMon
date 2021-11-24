using System.Collections;
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

    public void SetData(Mon mon)
    {
        _mon = mon;

        nameText.text = mon.Base.Name;
        levelText.text = "Lvl " + mon.Level;
        hpBar.SetHP((float) mon.HP / mon.MaxHp);
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
