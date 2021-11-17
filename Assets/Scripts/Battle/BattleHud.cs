using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] TextMeshProUGUI statusText;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;
    Dictionary<ConditionID, Color> statusColors;

    Mon _mon;

    public void SetData(Mon mon)
    {
        _mon = mon;

        nameText.text = mon.Base.Name;
        levelText.text = "Lvl " + mon.Level;
        hpBar.SetHP((float) mon.HP / mon.MaxHp);

        statusColors = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn, psnColor},
            {ConditionID.brn, brnColor},
            {ConditionID.slp, slpColor},
            {ConditionID.par, parColor},
            {ConditionID.frz, frzColor}
        };

        SetStatusText();
        _mon.OnStatusChanged += SetStatusText;
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

    public IEnumerator UpdateHP()
    {
        if(_mon.HpChanged)
        {
            yield return hpBar.SetHPSmooth((float) _mon.HP / _mon.MaxHp);
            _mon.HpChanged = false;
        }
    }
}
