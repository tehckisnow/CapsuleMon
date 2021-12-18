using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Nicknamer : MonoBehaviour
{
    [SerializeField] string dialogTextAsk = "Would you like to give a mon a new nickname?";
    [SerializeField] string dialogTextDecline = "Okay, if you change your mind, let me know!";
    
    private Mon monToName;

    public void Interact()
    {
        StartCoroutine(InteractCoroutine());
    }

    public IEnumerator InteractCoroutine()
    {
        yield return null;
        Action yesAction = () =>
        {
            GameController.Instance.PartyScreen.gameObject.SetActive(true);
            GameController.Instance.state = GameState.ChooseMonToNickname;
        };
        Action noAction = () =>
        {
            StartCoroutine(DialogManager.Instance.ShowDialogText(dialogTextDecline));
            Action revertToFreeRoam = () => GameController.Instance.state = GameState.FreeRoam;
            StartCoroutine(GameController.Instance.WhenDialogClose(revertToFreeRoam));
        };
        GameController.Instance.OpenConfirmationMenu(dialogTextAsk, yesAction, noAction);
    }

    public void SetMon(Mon mon)
    {
        monToName = mon;
    }

    IEnumerator NameMon()
    {
        yield return DialogManager.Instance.ShowDialogText(dialogTextAsk);
        GameController.Instance.OpenNicknameMenu(monToName, GameState.FreeRoam);
    }
}
