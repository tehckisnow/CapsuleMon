using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;

    Mon _mon;

    public void SetData(Mon mon)
    {
        _mon = mon;

        nameText.text = mon.Base.Name;
        levelText.text = "Lvl " + mon.Level;
        hpBar.SetHP((float) mon.HP / mon.MaxHp);
    }
}
