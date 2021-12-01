using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StepTrigger : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] UnityEvent action;

    public bool TriggerRepeatedly => false;

    public void OnPlayerTriggered(PlayerController player)
    {
        action?.Invoke();
    }
}
