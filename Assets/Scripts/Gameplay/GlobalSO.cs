using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Global", menuName ="Global/create new GlobalSO")]
public class GlobalSO : ScriptableObject
{
    [SerializeField] string saveSlotName = "saveSlot1";
    public string SaveSlotName => saveSlotName;

    [SerializeField] string playerID;
    public string PlayerID => playerID;


}
