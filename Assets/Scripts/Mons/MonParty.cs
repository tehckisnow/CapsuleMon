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
