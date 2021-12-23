using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour, Interactable, ISavable
{
    [SerializeField] ItemBase item;

    public bool Used { get; set; } = false;

    public IEnumerator Interact(Transform initiator)
    {
        if(!Used)
        {
            PlayerController player = initiator.GetComponent<PlayerController>();

            if(item is MoneyItem)
            {
                MoneyItem moneyItem = item as MoneyItem;
                yield return DialogManager.Instance.QueueDialogTextCoroutine($"{player.Name} found {moneyItem.GetName()}!");
                player.Money += moneyItem.Amount;
            }
            else
            {
                player.GetComponent<Inventory>().AddItem(item);
                yield return DialogManager.Instance.QueueDialogTextCoroutine($"{player.Name} found {item.Name}!");
            }

            Used = true;
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    // ISavable
    public object CaptureState()
    {
        return Used;
    }

    public void RestoreState(object state)
    {
        Used = (bool)state;
        if(Used)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
