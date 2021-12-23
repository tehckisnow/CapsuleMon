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
    
    public const int MAXPARTYSIZE = 6;

    public event Action OnUpdated;

    public static MonParty GetPlayerParty()
    {
        return FindObjectOfType<PlayerController>().GetComponent<MonParty>();
    }

    private void Awake()
    {
        foreach(var mon in mons)
        {
            if(!mon.Initialized)
            {
                mon.Init();
            }
        }
    }

    public Mon GetHealthyMon()
    {
        return mons.Where(x => x.HP > 0).FirstOrDefault();
    }

    public void AddMon(Mon newMon)
    {
        if(mons.Count < MonParty.MAXPARTYSIZE)
        {
            mons.Add(newMon);
            OnUpdated?.Invoke();
        }
        else
        {
            //TODO: add to the PC once that's implemented
            if(!newMon.Initialized)
            {
                newMon.Init();
            }
            SendToStorage(newMon);
        }
    }

    public bool LastMon()
    {
        if(mons.Count < 2)
        {
            string message = "You probably shouldn't get rid of your last mon...";
            //StartCoroutine(DialogManager.Instance.ShowDialogText(message));
            StartCoroutine(DialogManager.Instance.QueueDialogTextCoroutine(message));
            return true;
        }
        else
        {
            return false;
        }
    }

    public void OpenDepositConfirmation(Mon mon)
    {
        if(LastMon())
        {
            //GameController.Instance.state = GameState.PartyScreen;
            GameController.Instance.RevertFromDialogTo(GameState.PartyScreen);
            return;
        }
        
        string message = $"Do you want to deposit {mon.Name}?";
        Action yesAction = () =>
        {
            void RevertState()
            {
                DialogManager.Instance.OnDialogFinished -= RevertState;
                GameController.Instance.state = GameState.PartyScreen;
            }
            GameController.Instance.state = GameState.Dialog;
            DialogManager.Instance.OnDialogFinished += RevertState;
            GetPlayerParty().SendToStorage(mon);
        };
        Action noAction = () =>
        {
            GameController.Instance.state = GameState.PartyScreen;
        };
        GameController.Instance.OpenConfirmationMenu(message, yesAction, noAction);
    }

    public void SendToStorage(Mon mon)
    {
        MonStorage.Instance.AddMon(mon);
        RemoveMon(mon);
        //StartCoroutine(DialogManager.Instance.ShowDialogText($"{mon.Name} has been sent to the digital storage system."));
        StartCoroutine(DialogManager.Instance.QueueDialogTextCoroutine($"{mon.Name} has been sent to the digital storage system."));
    }

    public void AddMon(Mon newMon, int index)
    {
        if(mons.Count < MonParty.MAXPARTYSIZE && index < MonParty.MAXPARTYSIZE - 1)
        {
            mons.Insert(index, newMon);
            OnUpdated?.Invoke();
        }
        else
        {
            //TODO: PC
            SendToStorage(newMon);
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
    }

    //I Added this to fix name not updating in partylist after evolving with evolutionItem
    //this is to be able to invoke OnUpdated from outside of MonParty; particularly in the EvolutionManager
    public void UpdateParty()
    {
        OnUpdated?.Invoke();
    }
}
