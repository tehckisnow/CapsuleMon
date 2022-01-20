using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealMons : MonoBehaviour
{
    public static HealMons Instance;

    private void Awake()
    {
        Instance = this;
    }

    //the static function HealPlayerParty can't be linked to nurses in Inspector, so this was created
    public void HealPlayerMonParty()
    {
        HealPlayerParty();
    }

    public static void HealPlayerParty()
    {
        MonParty party = MonParty.GetPlayerParty();
        foreach(Mon mon in party.Mons)
        {
            mon.CureStatus();
            mon.IncreaseHP(mon.MaxHp);
            mon.isFainted = false;
            foreach(Move move in mon.Moves)
            {
                move.PP = move.Base.PP;
            }
        }
    }

    public void HealParty(MonParty party)
    {
        foreach(Mon mon in party.Mons)
        {
            mon.CureStatus();
            mon.IncreaseHP(mon.MaxHp);
            mon.isFainted = false;
            foreach(Move move in mon.Moves)
            {
                move.PP = move.Base.PP;
            }
        }
    }

    public MonParty FindPlayerParty()
    {
        return MonParty.GetPlayerParty();
    }
}
