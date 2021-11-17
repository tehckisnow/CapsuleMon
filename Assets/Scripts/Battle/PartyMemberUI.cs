using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color highlightedColor;

    private Color normalColor;

    private Mon _mon;

    private void Start()
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
            nameText.color = highlightedColor;
        }
        else
        {
            nameText.color = normalColor;
        }
    }
}
