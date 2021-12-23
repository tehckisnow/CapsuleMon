using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GymLeaderReward : MonoBehaviour
{
    [SerializeField] Dialog badgeDialog;
    [SerializeField] KeyItem badge;
    [SerializeField] Dialog rewardDialog;
    [SerializeField] ItemBase rewardItem;
    [SerializeField] Dialog endDialog;
    [SerializeField] float delay = 1f;

    TrainerController trainer;

    private void Start()
    {
        trainer = GetComponent<TrainerController>();
        trainer.afterBattleAction += Activate;
    }

    public void Activate()
    {
        StartCoroutine(BeginDialogs(delay));
        trainer.afterBattleAction -= Activate;
    }

    private IEnumerator BeginDialogs(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        var player = PlayerController.Instance;
        var inventory = player.GetComponent<Inventory>();
        if(badgeDialog != null)
        {
            //yield return DialogManager.Instance.ShowDialog(badgeDialog);
            yield return DialogManager.Instance.QueueDialogCoroutine(badgeDialog);
        }
        if(badge != null)
        {
            inventory.AddItem(badge);
            //yield return DialogManager.Instance.ShowDialogText($"{player.Name} received {badge.Name}!");
            yield return DialogManager.Instance.QueueDialogTextCoroutine($"{player.Name} received {badge.Name}!");
        }
        if(rewardDialog != null)
        {
            //yield return DialogManager.Instance.ShowDialog(rewardDialog);
            yield return DialogManager.Instance.QueueDialogCoroutine(rewardDialog);
        }
        if(rewardItem != null)
        {
            inventory.AddItem(rewardItem);
            //yield return DialogManager.Instance.ShowDialogText($"{player.Name} received {rewardItem.Name}!");
            yield return DialogManager.Instance.QueueDialogTextCoroutine($"{player.Name} received {rewardItem.Name}!");
        }
        if(endDialog != null)
        {
            //yield return DialogManager.Instance.ShowDialog(endDialog);
            yield return DialogManager.Instance.QueueDialogCoroutine(endDialog);
        }
    }
}
