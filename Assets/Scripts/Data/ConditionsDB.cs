using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionsDB
{
    public static void Init()
    {
        foreach(var kvp in Conditions)
        {
            var conditionId = kvp.Key;
            var condition = kvp.Value;

            condition.Id = conditionId;
        }
    }

    public static Dictionary<ConditionID, Condition> Conditions { get; set; } = new Dictionary<ConditionID, Condition>()
    {
        {
            ConditionID.psn,
            new Condition()
            {
                Name = "Poison",
                StartMessage = "has been poisoned",
                OnAfterTurn = (Mon mon) => 
                {
                    mon.UpdateHP(mon.MaxHp / 8);
                    mon.StatusChanges.Enqueue($"{mon.Base.Name} is hurt due to poison");
                }
            }
        },
        {
            ConditionID.brn,
            new Condition()
            {
                Name = "Burn",
                StartMessage = "has been burned",
                OnAfterTurn = (Mon mon) => 
                {
                    mon.UpdateHP(mon.MaxHp / 16);
                    mon.StatusChanges.Enqueue($"{mon.Base.Name} is hurt due to burn");
                }
            }
        },
        {
            ConditionID.par,
            new Condition()
            {
                Name = "Paralyzed",
                StartMessage = "has been paralyzed",
                OnBeforeMove = (Mon mon) => 
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        mon.StatusChanges.Enqueue($"{mon.Base.Name} is paralyzed and cannot move");
                        return false;
                    }
                    return true;
                }
            }
        },
        {
            ConditionID.frz,
            new Condition()
            {
                Name = "Frozen",
                StartMessage = "has been frozen",
                OnBeforeMove = (Mon mon) => 
                {
                    if(Random.Range(1, 5) == 1)
                    {
                        mon.CureStatus();
                        mon.StatusChanges.Enqueue($"{mon.Base.Name} is no longer frozen");
                        return true;
                    }
                    return false;
                }
            }
        },
        {
            ConditionID.slp,
            new Condition()
            {
                Name = "Sleep",
                StartMessage = "has fallen asleep",
                OnStart = (Mon mon) =>
                {
                    //sleep for 1-3 turns
                    mon.StatusTime = Random.Range(1, 4);
                    Debug.Log($"Will be asleep for {mon.StatusTime} moves");
                },
                OnBeforeMove = (Mon mon) => 
                {
                    if(mon.StatusTime <= 0)
                    {
                        mon.CureStatus();
                        mon.StatusChanges.Enqueue($"{mon.Base.Name} woke up!");
                        return true;
                    }

                    mon.StatusTime--;
                    mon.StatusChanges.Enqueue($"{mon.Base.Name} is fast asleep");
                    return false;
                }
            }
        },

        // Volatile Status Conditions
        {
            ConditionID.confusion,
            new Condition()
            {
                Name = "Confusion",
                StartMessage = "has become confused",
                OnStart = (Mon mon) =>
                {
                    mon.VolatileStatusTime = Random.Range(1, 5);
                    Debug.Log($"Will be confused for {mon.VolatileStatusTime} moves");
                },
                OnBeforeMove = (Mon mon) => 
                {
                    if(mon.VolatileStatusTime <= 0)
                    {
                        mon.CureVolatileStatus();
                        mon.StatusChanges.Enqueue($"{mon.Base.Name} has snapped out of confusion!");
                        return true;
                    }

                    mon.VolatileStatusTime--;

                    //50% chance to do a move
                    if(Random.Range(1, 3) == 1)
                    {
                        return true;
                    }

                    //hurt by confusion
                    mon.StatusChanges.Enqueue($"{mon.Base.Name} is confused");
                    mon.UpdateHP(mon.MaxHp / 8);
                    mon.StatusChanges.Enqueue($"It hurt itself in its confusion");
                    return false;
                }
            }
        }
    };

}

public enum ConditionID
{
    none, psn, brn, slp, par, frz,
    confusion
}
