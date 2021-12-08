using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class MonParty : MonoBehaviour
{
    [SerializeField] List<Mon> mons;
    public List<Mon> Mons {
        get { return mons; }
        set 
        { 
            mons = value; 
            OnUpdated?.Invoke();
        }
    }

    public event Action OnUpdated;

    public static MonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<MonParty>();
    }

    private void Awake()
    {
        foreach(var mon in mons)
        {
            mon.Init();
        }
    }

    public Mon GetHealthyMon()
    {
        return mons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddMon(Mon newMon)
    {
        if(mons.Count < 6)
        {
            mons.Add(newMon);
            OnUpdated?.Invoke();
        }
        else
        {
            //TODO: add to the PC once that's implemented

        }
    }

    public void AddMon(Mon newMon, int index)
    {
        if(mons.Count < 6 && index < 5)
        {
            mons.Insert(index, newMon);
            OnUpdated?.Invoke();
        }
        else
        {
            //TODO: PC
        }
    }

    public Mon RemoveMon(Mon mon)
    {
        mons.Remove(mon);
        OnUpdated?.Invoke();
        return mon;
    }

    //this will reorder the indexes of the other mons
    public Mon RemoveMon(int index)
    {
        Mon mon = mons[index];
        mons.RemoveAt(index);
        OnUpdated?.Invoke();
        return mon;
    }

    public void SwitchMons(Mon monA, Mon monB)
    {
        int firstIndex = mons.IndexOf(monA);
        Mon first = mons[firstIndex];
        
        int secondIndex = mons.IndexOf(monB);
        Mon second = mons[secondIndex];

        //mons[firstIndex] = mons[secondIndex];
        //alternative to below
        
        RemoveMon(firstIndex);
        AddMon(second, firstIndex);

        //mons[secondIndex] = first;

        RemoveMon(secondIndex);
        AddMon(first, secondIndex);
    }
    
    public IEnumerator CheckForEvolutions()
    {
        foreach(var mon in mons)
        {
            var evolution = mon.CheckForEvolution();
            if(evolution != null)
            {
                yield return EvolutionManager.i.Evolve(mon, evolution);
            }
        }

        OnUpdated?.Invoke();
    }

    //I Added this to fix name not updating in partylist after evolving with evolutionItem
    //this is to be able to invoke OnUpdated from outside of MonParty; particularly in the EvolutionManager
    public void UpdateParty()
    {
        OnUpdated?.Invoke();
    }
}
