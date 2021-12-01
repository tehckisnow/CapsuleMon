using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealMons : MonoBehaviour
{
    public void HealParty()
    {
        Debug.Log("initiating healing");
        HealParty(FindPlayerParty());
    }

    public void HealParty(MonParty party)
    {
        foreach(Mon mon in party.Mons)
        {
            Debug.Log($"{mon.Name}");
            mon.CureStatus();
            mon.IncreaseHP(mon.MaxHp);
            foreach(Move move in mon.Moves)
            {
                move.PP = move.Base.PP;
            }
            Debug.Log($"{mon.Name} HP: {mon.HP}/{mon.Base.MaxHp}");
        }
    }

    public MonParty FindPlayerParty()
    {
        Debug.Log("Finding party");
        return MonParty.GetPlayerParty();
    }
}
