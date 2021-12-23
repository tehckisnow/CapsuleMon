using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGiver : MonoBehaviour, ISavable
{
    [SerializeField] ItemBase item;
    [SerializeField] int count = 1;
    [SerializeField] Dialog dialog;

    private bool used = false;

    public IEnumerator GiveItem(PlayerController player)
    {
        //yield return DialogManager.Instance.ShowDialog(dialog);
        yield return DialogManager.Instance.QueueDialogCoroutine(dialog);

        player.GetComponent<Inventory>().AddItem(item, count);
        
        used = true;

        string dialogText = $"{player.Name} recieved {item.Name}";
        if(count > 1)
        {
            dialogText = $"{player.Name} recieved {count} {item.Name}s";
        }

        //yield return DialogManager.Instance.ShowDialogText(dialogText);
        yield return DialogManager.Instance.QueueDialogTextCoroutine(dialogText);
    }

    public bool CanBeGiven()
    {
        return item != null && count > 0 && !used;
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
