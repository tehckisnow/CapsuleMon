using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonDB
{
    static Dictionary<string, MonBase> mons;

    public static void Init()
    {
        mons = new Dictionary<string, MonBase>();

        //this loads all objects of type MonBase from within the "Resources" folder
        var monArray = Resources.LoadAll<MonBase>("");
        foreach(var mon in monArray)
        {
            if(mons.ContainsKey(mon.Name))
            {
                Debug.LogError($"There are two mons with the name {mon.Name}");
                continue;
            }

            mons[mon.Name] = mon;
        }
    }

    public static MonBase GetMonByName(string name)
    {
        if(!mons.ContainsKey(name))
        {
            Debug.LogError($"Mon with name {name} not found in the database");
            return null;
        }
        else
        {
            return mons[name];
        }
    }
}
