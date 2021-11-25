﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealMons : MonoBehaviour
{
    public void HealParty()
    {
        HealParty(FindPlayerParty());
    }

    public void HealParty(MonParty party)
    {
        foreach(Mon mon in party.Mons)
        {
            mon.HP = mon.MaxHp;
            mon.CureStatus();
            foreach(Move move in mon.Moves)
            {
                move.PP = move.Base.PP;
            }
        }
    }

    public MonParty FindPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<MonParty>();
    }
}