using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new capsule item")]
public class CapsuleItem : ItemBase
{
    
    public override bool Use(Mon mon)
    {
        return true;
    }
}
