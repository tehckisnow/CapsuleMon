using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class BattleHud : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] HPBar hpBar;
    [SerializeField] TextMeshProUGUI statusText;
    [SerializeField] GameObject expBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color slpColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;
    Dictionary<ConditionID, Color> statusColors;

    Mon _mon;

    public void SetData(Mon mon)
    {
        ClearData();

        _mon = mon;

        nameText.text = mon.Base.Name;
        SetLevel();
        hpBar.SetHP((float) mon.HP / mon.MaxHp);
        SetExp();

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
        _mon.OnHPChanged += UpdateHP;
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

    public void SetLevel()
    {
        levelText.text = "Lvl " + _mon.Level;
    }

    public void SetExp()
    {
        if(expBar == null)
        {
            return;
        }

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if(expBar == null)
        {
            yield break;
        }

        if(reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }

    private float GetNormalizedExp()
    {
        int currentLevelExp = _mon.Base.GetExpForLevel(_mon.Level);
        int nextLevelExp = _mon.Base.GetExpForLevel(_mon.Level + 1);

        float normalizedExp = (float)(_mon.Exp - currentLevelExp) / (nextLevelExp - currentLevelExp);
        return Mathf.Clamp01(normalizedExp);
    }

    public void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float) _mon.HP / _mon.MaxHp);
    }

    //waits until HP is finished updating
    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }

    public void ClearData()
    {
        if(_mon != null)
        {
            _mon.OnHPChanged -= UpdateHP;
            _mon.OnStatusChanged -= SetStatusText;
        }
    }
}
