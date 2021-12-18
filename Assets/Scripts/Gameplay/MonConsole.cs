using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonConsole : MonoBehaviour, Interactable
{
    [SerializeField] string dialog = "Booted up the Mon Storage System...";
    private MonStorage monStorage;

    private void Awake()
    {
        monStorage = MonStorage.Instance;
    }

    public IEnumerator Interact(Transform player)
    {
        yield return ShowDialog();
        GameController.Instance.OpenMonStorage();
    }

    IEnumerator ShowDialog()
    {
        yield return DialogManager.Instance.ShowDialogText(dialog);
    }
}
