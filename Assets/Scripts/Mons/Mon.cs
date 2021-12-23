using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Mon
{
    [SerializeField] MonBase _base;
    [SerializeField] int level;

    public Mon(MonBase pBase, int plevel)
    {
        _base = pBase;
        level = plevel;

        Init();
    }

    public MonBase Base {
        get { return _base; }
    }
    public int Level {
        get { return level; }
    }

    public string Name { get; set; }

    public int Exp { get; set; }

    public int HP { get; set; }

    public List<Move> Moves { get; set; }

    public Move CurrentMove { get; set; }

    public Dictionary<Stat, int> Stats { get; private set; }
    
    public Dictionary<Stat, int> StatBoosts { get; private set; }

    public Condition Status { get; private set; }
    public int StatusTime { get; set; }

    public Condition VolatileStatus { get; private set; }
    public int VolatileStatusTime { get; set; }
    
    public Queue<string> StatusChanges { get; private set; }

    public event System.Action OnStatusChanged;
    public event System.Action OnHPChanged;

    private bool initialized = false;
    public bool Initialized => initialized;

    //currently just used for poison outside of battle; expand for other uses
    public bool isFainted = false;

    public void Init()
    {
        //generate moves
        Moves = new List<Move>();
        foreach(var move in Base.LearnableMoves)
        {
            if(move.Level <= Level)
            {
                Moves.Add(new Move(move.Base));

                if(Moves.Count >= MonBase.MaxNumberOfMoves)
                {
                    break;
                }
            }
        }

        Exp = Base.GetExpForLevel(Level);

        CalculateStats();
        HP = MaxHp;

        Name = Base.Name;
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        Status = null;
        VolatileStatus = null;
        isFainted = false;

        initialized = true;
    }

    private void ResetStatBoost()
    {
        StatBoosts = new Dictionary<Stat, int>()
        {
            {Stat.Attack, 0},
            {Stat.Defense, 0},
            {Stat.SpAttack, 0},
            {Stat.SpDefense, 0},
            {Stat.Speed, 0},
            
            {Stat.Accuracy, 0},
            {Stat.Evasion, 0}
        };
    }

    private int GetStat(Stat stat)
    {
        int statVal = Stats[stat];
        //apply stat boost
        int boost = StatBoosts[stat];
        var boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f};
        if(boost >= 1)
        {
            statVal = Mathf.FloorToInt(statVal * boostValues[boost]);
        }
        else
        {
            statVal = Mathf.FloorToInt(statVal / boostValues[-boost]);
        }

        return statVal;
    }

    public Mon(MonSaveData saveData)
    {
        _base = MonDB.GetObjectByName(saveData.name);
        level = saveData.level;
        Name = saveData.nickname;
        HP = saveData.hp;
        Exp = saveData.exp;
        isFainted = saveData.fainted;

        if(saveData.statusId != null)
        {
            Status = ConditionsDB.Conditions[saveData.statusId.Value];
        }
        else
        {
            Status = null;
        }

        //moves
        Moves = saveData.moves.Select(s => new Move(s)).ToList();

        CalculateStats();
        StatusChanges = new Queue<string>();
        ResetStatBoost();
        VolatileStatus = null;

    }

    public MonSaveData GetSaveData()
    {
        var saveData = new MonSaveData()
        {
            nickname = Name,
            name = Base.name,
            level = Level,
            hp = HP,
            exp = Exp,
            statusId = Status?.Id,
            moves = Moves.Select(m => m.GetSaveData()).ToList(),
            fainted = isFainted
        };

        return saveData;
    }

    private void RecalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        int oldMaxHP = MaxHp;
        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level;

        HP += MaxHp - oldMaxHP;
        
        HP = Mathf.Clamp(HP, 0, MaxHp);
    }

    private void CalculateStats()
    {
        Stats = new Dictionary<Stat, int>();
        Stats.Add(Stat.Attack, Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5);
        Stats.Add(Stat.Defense, Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5);
        Stats.Add(Stat.SpAttack, Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5);
        Stats.Add(Stat.SpDefense, Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5);
        Stats.Add(Stat.Speed, Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5);

        MaxHp = Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level;
        HP = Mathf.Clamp(HP, 0, MaxHp);
    }

    public void ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach(var statBoost in statBoosts)
        {
            var stat = statBoost.stat;
            var boost = statBoost.boost;

            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);

            if(boost > 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} rose!");
            }
            else if(boost < 0)
            {
                StatusChanges.Enqueue($"{Base.Name}'s {stat} fell!");
            }
        }
    }

    public bool CheckForLevelUp()
    {
        if(Exp >= Base.GetExpForLevel(level + 1))
        {
            ++level;
            RecalculateStats();
            return true;
        }
        else
        {
            return false;
        }
    }

    // // deprecated: remove this
    // public LearnableMove GetLearnableMoveAtCurrentLevel()
    // {
    //     return Base.LearnableMoves.Where(x => x.Level == level).FirstOrDefault();
    // }

    // this replaces the above
    public List<LearnableMove> GetLearnableMovesAtCurrentLevel()
    {
        List<LearnableMove> newList = new List<LearnableMove>();
        foreach(LearnableMove move in Base.LearnableMoves)
        {
            if(move.Level == level)
            {
                newList.Add(move);
            }
        }
        return newList;
    }

    public void LearnMove(MoveBase moveToLearn)
    {
        if(Moves.Count > MonBase.MaxNumberOfMoves)
        {
            return;
        }
        else
        {
            Moves.Add(new Move(moveToLearn));
        }
    }

    public bool HasMove(MoveBase moveToCheck)
    {
        return Moves.Count(mon => mon.Base == moveToCheck) > 0;
    }

    public Evolution CheckForEvolution()
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredLevel <= level && e.RequiredLevel != 0);
    }

    public Evolution CheckForEvolution(ItemBase item)
    {
        return Base.Evolutions.FirstOrDefault(e => e.RequiredItem == item);
    }

    public void Evolve(Evolution evolution)
    {
        _base = evolution.EvolveInto;
        RecalculateStats();
    }

    public int Attack
    {
        get { return GetStat(Stat.Attack); }
    }

    public int Defense
    {
        get { return GetStat(Stat.Defense); }
    }

    public int SpAttack
    {
        get { return GetStat(Stat.SpAttack); }
    }

    public int SpDefense
    {
        get { return GetStat(Stat.SpDefense); }
    }

    public int Speed
    {
        get { return GetStat(Stat.Speed); }
    }

    public int MaxHp
    {
        get; private set;
    }

    public DamageDetails TakeDamage(Move move, Mon attacker)
    {
        float critical = 1f;
        if(Random.value * 100f <= 6.25f)
        {
            critical = 2f;
        }

        float typeEffectiveness = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) * TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        var damageDetails = new DamageDetails()
        {
            TypeEffectiveness = typeEffectiveness,
            Critical = critical,
            Fainted = false
        };

        float attack = (move.Base.Category == MoveCategory.Special) ? attacker.SpAttack : attacker.Attack;
        float defense = (move.Base.Category == MoveCategory.Special) ? SpDefense : Defense;

        float modifiers = Random.Range(0.85f, 1f) * typeEffectiveness * critical;
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        UpdateHP(damage);

        return damageDetails;
    }

    //! in tut he renamed this to "DecreaseHP", which would require updating it in multiple other scripts
    public void UpdateHP(int damage)
    {
        HP = Mathf.Clamp(HP - damage, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public void IncreaseHP(int amount)
    {
        HP = Mathf.Clamp(HP + amount, 0, MaxHp);
        OnHPChanged?.Invoke();
    }

    public void SetStatus(ConditionID conditionId)
    {
        if(Status != null)
        {
            return;
        }

        Status = ConditionsDB.Conditions[conditionId];
        Status?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {Status.StartMessage}");
        OnStatusChanged?.Invoke();
    }

    public void CureStatus()
    {
        Status = null;
        OnStatusChanged?.Invoke();
    }

    public void SetVolatileStatus(ConditionID conditionId)
    {
        if(VolatileStatus != null)
        {
            return;
        }

        VolatileStatus = ConditionsDB.Conditions[conditionId];
        VolatileStatus?.OnStart?.Invoke(this);
        StatusChanges.Enqueue($"{Base.Name} {VolatileStatus.StartMessage}");
    }

    public void CureVolatileStatus()
    {
        VolatileStatus = null;
    }

    public Move GetRandomMove()
    {
        //! will null ref error if there are no moves with PP left
        var movesWithPP = Moves.Where(x => x.PP > 0).ToList();

        int r = Random.Range(0, movesWithPP.Count);
        return movesWithPP[r];
    }

    public bool OnBeforeMove()
    {
        bool canPerformMove = true;
        if(Status?.OnBeforeMove != null)
        {
            if(!Status.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        if(VolatileStatus?.OnBeforeMove != null)
        {
            if(!VolatileStatus.OnBeforeMove(this))
            {
                canPerformMove = false;
            }
        }

        return canPerformMove;
    }

    public void OnAfterTurn()
    {
        Status?.OnAfterTurn?.Invoke(this);
        VolatileStatus?.OnAfterTurn?.Invoke(this);
    }

    public void OnBattleOver()
    {
        VolatileStatus = null;
        ResetStatBoost();
    }

    //-------------------------------------------------------
    // UI

    public IEnumerator CheckForEvolutionMove()
    {
        yield return TryToLearnMove(true);
    }

    //These two are used by TryToLearnMove for iterating through multiple moves learned at once (upon levelup, evolution, etc.)
    private bool readyForMove = false;
    public bool ReadyForMove => readyForMove;
    public void SetReadyForMove()
    {
        readyForMove = true;
    }

    public IEnumerator TryToLearnMove(bool evo=false)
    {
        var moves = GetLearnableMovesAtCurrentLevel();
        if(evo)
        {
            moves = new List<LearnableMove>();
            foreach(MoveBase moveBase in Base.MovesLearnedUponEvolution)
            {
                var move = new LearnableMove()
                {
                    Base = moveBase,
                    Level = 1
                };
                moves.Add(move);
            }
        }
        if(moves == null)
        {
            yield break;
        }
        readyForMove = false;
        foreach(LearnableMove move in moves)
        {
            if(move != null)
            {
                if(Moves.Count < MonBase.MaxNumberOfMoves)
                {
                    LearnMove(move.Base);
                    //yield return DialogManager.Instance.ShowDialogText($"{Name} learned {move.Base.Name}");
                    yield return DialogManager.Instance.QueueDialogTextCoroutine($"{Name} learned {move.Base.Name}");
                    readyForMove = true;
                }
                else
                {
                    //yield return DialogManager.Instance.ShowDialogText($"{Name} is trying to learn {move.Base.Name}");
                    yield return DialogManager.Instance.QueueDialogTextCoroutine($"{Name} is trying to learn {move.Base.Name}");
                    //yield return DialogManager.Instance.ShowDialogText($"But it can't learn more than {MonBase.MaxNumberOfMoves} moves");
                    yield return DialogManager.Instance.QueueDialogTextCoroutine($"But it can't learn more than {MonBase.MaxNumberOfMoves} moves");
                    //yield return DialogManager.Instance.ShowDialogText($"Choose a move to forget");
                    yield return DialogManager.Instance.QueueDialogTextCoroutine($"Choose a move to forget");
                    GameController.Instance.OpenMoveSelectionUI(this, move.Base);
                }
            }
            yield return new WaitUntil(() => readyForMove);
        }
        readyForMove = false;
    }
}

public class DamageDetails
{
    public bool Fainted { get; set; }
    public float Critical { get; set; }
    public float TypeEffectiveness { get; set; }
}

[System.Serializable]
public class MonSaveData
{
    public string nickname;
    public string name;
    public int level;
    public int hp;
    public int exp;
    public ConditionID? statusId;
    public List<MoveSaveData> moves;
    public bool fainted;
}