using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryItem : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] Dialog dialog;

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.IsMoving = false; //! this is from the tut to fix an error that I didn't have
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
    }
    
    public bool TriggerRepeatedly => false;
}
