using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Items/Create new money item")]
public class MoneyItem : ItemBase
{
    [SerializeField] int amount;
    public int Amount => amount;

    public string GetName()
    {
        return $"${amount.ToString()}";
    }

    public override bool Use(Mon mon)
    {
        return false;
    }
}
