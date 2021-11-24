using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;

    public void Interact(Transform player)
    {
        StartCoroutine(ShowDialog());
    }

    IEnumerator ShowDialog()
    {
        yield return DialogManager.Instance.ShowDialog(dialog);
        yield return new WaitUntil(() => FindObjectOfType<GameController>().state != GameState.Dialog);
    }
}
