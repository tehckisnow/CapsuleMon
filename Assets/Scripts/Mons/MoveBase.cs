using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Move", menuName = "Mon/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] MonType type;
    [SerializeField] int power;
    [SerializeField] int accuracy;
    [SerializeField] int pp;

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public MonType Type
    {
        get { return type; }
    }

    public int Power
    {
        get { return power; }
    }

    public int Accuracy
    {
        get { return accuracy; }
    }

    public int PP
    {
        get { return pp; }
    }

    public bool IsSpecial
    {
        get {
            if(type == MonType.Fire || type == MonType.Water || type == MonType.Grass ||
            type == MonType.Ice || type == MonType.Electric || type == MonType.Dragon)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
