using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonGiver : MonoBehaviour, ISavable
{
    [SerializeField] Mon monToGive;
    [SerializeField] Dialog dialog;

    private bool used = false;

    public IEnumerator GiveMon(PlayerController player)
    {
        yield return DialogManager.Instance.ShowDialog(dialog);

        monToGive.Init();
        player.GetComponent<MonParty>().AddMon(monToGive);
        
        used = true;

        string dialogText = $"{player.Name} recieved {monToGive.Base.Name}";

        yield return DialogManager.Instance.ShowDialogText(dialogText);
    }

    public bool CanBeGiven()
    {
        return monToGive != null && !used;
    }

    // ISavable
    public object CaptureState()
    {
        return used;
    }

    public void RestoreState(object state)
    {
        used = (bool)state;
    }
}

