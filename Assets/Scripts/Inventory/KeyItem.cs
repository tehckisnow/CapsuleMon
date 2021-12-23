using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Items/Create new key item")]
public class KeyItem : ItemBase
{
    // [SerializeField] string name;
    // [SerializeField] string description;
    // [SerializeField] Sprite icon;

    // public override string Name => name;
    // public string Description => description;
    // public Sprite Icon => icon;

    public KeyItemType type;

    public override bool CanUseInBattle => false;
    public override bool CanUseOutsideBattle => true;

    public override bool IsReusable => true;

    public override bool Use(Mon mon)
    {
        return false;
    }
}

public enum KeyItemType { Generic, Badge, Bike }
