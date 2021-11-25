using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new capsule item")]
public class CapsuleItem : ItemBase
{
    [SerializeField] float catchRateModifier = 1;

    public float CatchRateModifier => catchRateModifier;

    public override bool Use(Mon mon)
    {
        if(GameController.Instance.State == GameState.Battle)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
