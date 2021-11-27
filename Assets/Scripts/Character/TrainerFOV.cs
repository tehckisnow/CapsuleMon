using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerFOV : MonoBehaviour, IPlayerTriggerable
{
    
    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.IsMoving = false;
        GameController.Instance.OnEnterTrainersView(GetComponentInParent<TrainerController>());
    }

    public bool TriggerRepeatedly => false;
}
