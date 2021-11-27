using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Items/Create new TM or HM")]
public class TmItem : ItemBase
{
    //base keyword gives property from parent (this is a normal part of inheritance)
    public override string Name => base.Name + $": {move.Name}";

    [SerializeField] MoveBase move;
    public MoveBase Move => move;

    [SerializeField] bool isHM;
    public bool IsHM => isHM;
    
    public override bool CanUseInBattle => false;

    public override bool IsReusable => isHM;

    public override bool Use(Mon mon)
    {
        //Learning moves handles from inventory UI, if it was learned then return true
        return mon.HasMove(move);
    }

    public bool CanBeTaught(Mon mon)
    {
        return mon.Base.LearnableByItems.Contains(move);
    }

}
