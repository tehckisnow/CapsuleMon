using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MonStorage : MonoBehaviour
{
    [SerializeField] List<Mon> mons = new List<Mon>();
    public List<Mon> Mons {
        get { return mons; }
        set 
        { 
            mons = value; 
        }
    }

    public static MonStorage Instance;

    private MonParty playerParty;

    private void Awake()
    {
        Instance = this;
        playerParty = MonParty.GetPlayerParty();

        if(mons.Count > 0)
        {
            foreach(var mon in mons)
            {
                if(!mon.Initialized)
                {
                    mon.Init();
                }
            }
        }
    }

    public void AddMon(Mon newMon)
    {
        mons.Add(newMon);
    }

    public void AddMon(Mon newMon, int index)
    {
        mons.Insert(index, newMon);
    }

    public Mon RemoveMon(Mon mon)
    {
        mons.Remove(mon);
        return mon;
    }

    //this will reorder the indexes of the other mons
    public Mon RemoveMon(int index)
    {
        Mon mon = mons[index];
        mons.RemoveAt(index);
        return mon;
    }

    public void SwitchMons(Mon monA, Mon monB)
    {
        int firstIndex = mons.IndexOf(monA);
        Mon first = mons[firstIndex];
        
        int secondIndex = mons.IndexOf(monB);
        Mon second = mons[secondIndex];
        
        RemoveMon(firstIndex);
        AddMon(second, firstIndex);

        RemoveMon(secondIndex);
        AddMon(first, secondIndex);
    }

    public void TakeMon(Mon mon)
    {
        if(Mons.Contains(mon))
        {
            if(playerParty.Mons.Count < MonParty.MAXPARTYSIZE)
            {
                var taken = RemoveMon(mon);
                playerParty.AddMon(taken);
                //StartCoroutine(DialogManager.Instance.ShowDialogText($"{taken.Name} has been added to your party"));
                StartCoroutine(DialogManager.Instance.QueueDialogTextCoroutine($"{taken.Name} has been added to your party"));
            }
            else
            {
                //StartCoroutine(DialogManager.Instance.ShowDialogText($"There is no room in your party!"));
                StartCoroutine(DialogManager.Instance.QueueDialogTextCoroutine($"There is no room in your party!"));
            }
        }
        else
        {
            Debug.LogError("Mon not found in MonStorage");
        }
    }
}
