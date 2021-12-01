using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : MonoBehaviour, Interactable
{
    [SerializeField] Dialog dialog;

    public IEnumerator Interact(Transform player)
    {
        yield return ShowDialog();
    }

    IEnumerator ShowDialog()
    {
        yield return DialogManager.Instance.ShowDialog(dialog);
        //yield return new WaitUntil(() => FindObjectOfType<GameController>().state != GameState.Dialog);
    }
}
