using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestFlags : MonoBehaviour
{    
    public static QuestFlags Instance;

    private Dictionary<string, bool> flags;

    [SerializeField] List<string> flagsList;

    private void Awake()
    {
        Instance = this;
        flags = new Dictionary<string, bool>();
        flagsList = new List<string>();
    }

    public void NewFlag(string name, bool state=false)
    {
        if(!flags.ContainsKey(name))
        {
            flags.Add(name, state);
            Debug.Log($"New Flag: {name}, {state}");
            PopulateFlagsList();
        }
        else
        {
            Debug.LogError($"flags already contains flag: {name}");
        }
    }

    public void SetFlag(string name, bool state=true)
    {
        if(flags.ContainsKey(name))
        {
            flags[name] = state;
            Debug.Log($"Set Flag: {name} to {state}");
            PopulateFlagsList();
        }
        else
        {
            //Debug.LogError($"flags does not contain flag: {name}");
            NewFlag(name, state);
        }
    }

    public bool GetFlag(string name)
    {
        if(flags.ContainsKey(name))
        {
            Debug.Log($"Checking Flag: {name} ; Exists");
            return flags[name];
        }
        {
            //Debug.LogError($"flags does not contain flag: {name}");
            return false;
        }
    }

    //fill flagsList with values from flags dictionary
    public void PopulateFlagsList()
    {
        flagsList = new List<string>(flags.Keys);
    }

    //fill flags dictionary with values from flagsList defaulting to defaultVal
    public void PopulateFlagsDictionary(bool defaultVal=false)
    {
        for(int i = 0; i < flagsList.Count; i++)
        {
            if(!flags.ContainsKey(flagsList[i]))
            {
                // values default to false
                flags.Add(flagsList[i], defaultVal);
            }
        }
    }
}
