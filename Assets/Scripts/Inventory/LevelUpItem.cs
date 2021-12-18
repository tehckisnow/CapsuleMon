using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Create new LevelUp item")]
public class LevelUpItem : ItemBase
{
    
    public override bool Use(Mon mon)
    {
        return true;
    }
}
