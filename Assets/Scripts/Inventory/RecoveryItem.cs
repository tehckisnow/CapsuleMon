using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new recovery item")]
public class RecoveryItem : ItemBase
{
    [Header("HP")]
    [SerializeField] int hpAmount;
    [SerializeField] bool restoreMaxHP;

    [Header("PP")]
    [SerializeField] int ppAmount;
    [SerializeField] bool restoreMaxPP;

    [Header("Status Conditions")]
    [SerializeField] ConditionID status;
    [SerializeField] bool recoverAllStatus;

    [Header("Revive")]
    [SerializeField] bool revive;
    [SerializeField] bool maxRevive;

    public override bool Use(Mon mon)
    {
        // Revive
        if(revive || maxRevive)
        {
            if(mon.HP > 0)
            {
                return false;
            }
            else if(revive)
            {
                mon.IncreaseHP(mon.MaxHp / 2);
            }
            else if(maxRevive)
            {
                mon.IncreaseHP(mon.MaxHp);
            }

            mon.CureStatus();
            
            return true;
        }

        // No other items can be used on fainted mons
        if(mon.HP == 0)
        {
            return false;
        }

        // Restore HP
        if(restoreMaxHP || hpAmount > 0)
        {
            if(mon.HP == mon.MaxHp)
            {
                return false;
            }
            if(restoreMaxHP)
            {
                mon.IncreaseHP(mon.MaxHp);
            }
            else
            {
                mon.IncreaseHP(hpAmount);
            }
        }

        // Recover status
        if(recoverAllStatus || status != ConditionID.none)
        {
            if(mon.Status == null && mon.VolatileStatus == null)
            {
                return false;
            }

            if(recoverAllStatus)
            {
                mon.CureStatus();
                mon.CureVolatileStatus();
            }
            else
            {
                if(mon.Status.Id == status)
                {
                    mon.CureStatus();
                }
                else if(mon.VolatileStatus.Id == status)
                {
                    mon.CureVolatileStatus();
                }
                else
                {
                    return false;
                }
            }
        }

        // Recover PP
        if(restoreMaxPP)
        {
            mon.Moves.ForEach(m => m.IncreasePP(m.Base.PP));
        }
        else if(ppAmount > 0)
        {
            mon.Moves.ForEach(m => m.IncreasePP(ppAmount));
        }

        return true;
    }
}
