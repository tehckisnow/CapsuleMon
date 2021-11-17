using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MonParty : MonoBehaviour
{
    [SerializeField] List<Mon> mons;
    public List<Mon> Mons {
        get { return mons; }
    }

    private void Start()
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
}
