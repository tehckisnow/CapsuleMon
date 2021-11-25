using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MonParty : MonoBehaviour
{
    [SerializeField] List<Mon> mons;
    public List<Mon> Mons {
        get { return mons; }
<<<<<<< HEAD
        set 
        { 
            mons = value; 
            OnUpdated?.Invoke();
        }
    }

    public event Action OnUpdated;

    public static MonParty PlayerParty { get; private set; }

    public static MonParty GetPlayerParty()
    {
        return PlayerParty;
        //return FindObjectOfType<PlayerController>().GetComponent<MonParty>();
=======
        set { mons = value; }
>>>>>>> parent of 9fcabf5 (Finished #58)
    }

    private void Awake()
    {
        foreach(var mon in mons)
        {
            mon.Init();
        }

        var player = FindObjectOfType<PlayerController>();
        if(player != null)
        {
            PlayerParty = player.GetComponent<MonParty>();
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
        }
        else
        {
            //TODO: add to the PC once that's implemented

        }
    }
}
