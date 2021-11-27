using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Mon", menuName = "Mon/Create new mon")]
public class MonBase : ScriptableObject
{
    [SerializeField] private string name;
    [TextArea]
    [SerializeField] private string description;

    [SerializeField] Sprite frontSprite;
    [SerializeField] Sprite backSprite;

    [SerializeField] MonType type1;
    [SerializeField] MonType type2;

    [SerializeField] int maxHp;
    [SerializeField] int attack;
    [SerializeField] int defense;
    [SerializeField] int spAttack;
    [SerializeField] int spDefense;
    [SerializeField] int speed;
    
    [SerializeField] int expYield;
    [SerializeField] GrowthRate growthRate;

    [SerializeField] int catchRate = 255;

    [SerializeField] List<LearnableMove> learnableMoves;
    [SerializeField] List<MoveBase> learnableByItems;
    public List<MoveBase> LearnableByItems => learnableByItems;

    public static int MaxNumberOfMoves { get; set; } = 4;

    public int GetExpForLevel(int level)
    {
        if(growthRate == GrowthRate.Fast)
        {
            return 4 * (level * level * level) / 5;
        }
        else if(growthRate == GrowthRate.MediumFast)
        {
            return level * level * level;
        }

        return -1;
    }

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Sprite FrontSprite
    {
        get { return frontSprite; }
    }

    public Sprite BackSprite
    {
        get { return backSprite; }
    }

    public MonType Type1
    {
        get { return type1; }
    }

    public MonType Type2
    {
        get { return type2; }
    }

    public int MaxHp
    {
        get { return maxHp; }
    }

    public int Attack
    {
        get { return attack; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int SpAttack
    {
        get { return spAttack; }
    }

    public int SpDefense
    {
        get { return spDefense; }
    }

    public int Speed
    {
        get { return speed; }
    }

    public int CatchRate => catchRate;
    
    public int ExpYield => expYield;

    public GrowthRate GrowthRate => growthRate;

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] MoveBase moveBase;
    [SerializeField] int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }
}

public enum MonType
{
    None,
    Normal,
    Fire,
    Water,
    Electric,
    Grass,
    Ice,
    Fighting,
    Poison,
    Ground,
    Flying,
    Psychic,
    Bug,
    Rock,
    Ghost,
    Dragon
}

public enum GrowthRate
{
    //Erratic,
    Fast, MediumFast,
    //MediumSlow, Slow, Fluctuating
}

public enum Stat
{
    Attack,
    Defense,
    SpAttack,
    SpDefense,
    Speed,

    //These two are not actual stats, they are just used to boost the moveAccuracy
    Accuracy,
    Evasion
}

public class TypeChart
{
    static float[][] chart =
    {
        //attack    defense->NOR,FIR,WAT,ELC,GRA,ICE,FIG,POI,GRO,FLY,PSY,BUG,ROC,GHO,DRA
        /*NOR*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f,.5f, 0f, 1f},
        /*FIR*/ new float[] { 1f,.5f,.5f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 1f, 2f,.5f, 1f,.5f},
        /*WAT*/ new float[] { 1f, 2f,.5f, 1f,.5f, 1f, 1f, 1f, 2f, 1f, 1f, 1f, 2f, 1f,.5f},
        /*ELC*/ new float[] { 1f, 1f, 2f,.5f,.5f, 1f, 1f, 1f, 0f, 2f, 1f, 1f, 1f, 1f,.5f},
        /*GRA*/ new float[] { 1f,.5f, 2f, 1f,.5f, 1f, 1f,.5f, 2f,.5f, 1f,.5f, 2f, 1f,.5f},
        /*ICE*/ new float[] { 1f,.5f,.5f, 1f, 2f,.5f, 1f, 1f, 2f, 2f, 1f, 1f, 1f, 1f, 2f},
        /*FIG*/ new float[] { 2f, 1f, 1f, 1f, 1f, 2f, 1f,.5f, 1f,.5f,.5f,.5f, 2f, 0f, 1f},
        /*POI*/ new float[] { 1f, 1f, 1f, 1f, 2f, 1f, 1f,.5f,.5f, 1f, 1f, 1f,.5f,.5f, 1f},
        /*GRO*/ new float[] { 1f, 2f, 1f, 2f,.5f, 1f, 1f, 2f, 1f, 0f, 1f,.5f, 2f, 1f, 1f},
        /*FLY*/ new float[] { 1f, 1f, 1f,.5f, 2f, 1f, 2f, 1f, 1f, 1f, 1f, 2f,.5f, 1f, 1f},
        /*PSY*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 2f, 2f, 1f, 1f,.5f, 1f, 1f, 1f, 1f},
        /*BUG*/ new float[] { 1f,.5f, 1f, 1f, 2f, 1f,.5f,.5f, 1f,.5f, 2f, 1f, 1f,.5f, 1f},
        /*ROC*/ new float[] { 1f, 2f, 1f, 1f, 1f, 2f,.5f, 1f,.5f, 2f, 1f, 1f, 1f, 1f, 1f},
        /*GHO*/ new float[] { 0f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f, 1f, 1f, 2f, 1f},
        /*DRA*/ new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 2f}
    };

    public static float GetEffectiveness(MonType attackType, MonType defenseType)
    {
        if(attackType == MonType.None || defenseType == MonType.None)
        {
            return 1;
        }

        int row = (int)attackType - 1;
        int col = (int)defenseType - 1;

        return chart[row][col];
    }
}
