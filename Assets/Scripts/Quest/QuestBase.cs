using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Quests/Create a new quest")]
public class QuestBase : ScriptableObject
{
    [SerializeField] string name;
    [SerializeField] string description;

    [SerializeField] Dialog startDialog;
    [SerializeField] List<ItemBase> startGiveItems;
    [SerializeField] Dialog inProgressDialog; //Optional
    [SerializeField] Dialog completedDialog;
    
    [SerializeField] List<string> requiredFlags;
    [SerializeField] ItemBase requiredItem;
    [SerializeField] bool promptToComplete = false;
    
    [SerializeField] string confirmationMessage = "Would you like to complete this quest?";
    [SerializeField] ItemBase rewardItem;
    [SerializeField] int rewardItemCount = 1;

    public string Name => name;
    public string Description => description;
    
    public Dialog StartDialog => startDialog;
    public List<ItemBase> StartGiveItems => startGiveItems;
    public Dialog InProgressDialog => inProgressDialog?.Lines?.Count > 0 ? inProgressDialog : startDialog;
    public Dialog CompletedDialog => completedDialog;
    
    public List<string> RequiredFlags => requiredFlags;
    public ItemBase RequiredItem => requiredItem;
    public bool PromptToComplete => promptToComplete;
    
    public string ConfirmationMessage => confirmationMessage;
    public ItemBase RewardItem => rewardItem;
    public int RewardItemCount => rewardItemCount;
}
