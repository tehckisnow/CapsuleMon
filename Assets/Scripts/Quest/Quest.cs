using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;
using System.Threading;

//not a good idea to put data that can change at runtime in a scriptable object, so this class exists
[System.Serializable]
public class Quest
{
    public QuestBase Base { get; private set; }
    public QuestStatus Status { get; private set; }
    
    private GameState thisState;
    private bool result;

    public Quest(QuestBase _base)
    {
        Base = _base;
    }

    public Quest(QuestSaveData saveData)
    {
        Base = QuestDB.GetObjectByName(saveData.name);
        Status = saveData.status;
    }

    public QuestSaveData GetSaveData()
    {
        var saveData = new QuestSaveData()
        {
            name = Base.name,
            status = Status
        };
        return saveData;
    }

    public IEnumerator StartQuest()
    {
        Status = QuestStatus.Started;
        //yield return DialogManager.Instance.ShowDialog(Base.StartDialog);
        yield return DialogManager.Instance.QueueDialogCoroutine(Base.StartDialog);
        
        if(Base.StartGiveItems.Count > 0)
        {
            Inventory inv = PlayerController.Instance.GetComponent<Inventory>();
            for(int i = 0; i < Base.StartGiveItems.Count; i++)
            {
                inv.AddItem(Base.StartGiveItems[i]);
                yield return DialogManager.Instance.QueueDialogTextCoroutine($"Received {Base.StartGiveItems[i].Name}!");
            }
        }

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public IEnumerator CompleteQuest(Transform player)
    {
        Status = QuestStatus.Completed;
        //yield return DialogManager.Instance.ShowDialog(Base.CompletedDialog);
        yield return DialogManager.Instance.QueueDialogCoroutine(Base.CompletedDialog);
        

        var inventory = Inventory.GetInventory();
        if(Base.RequiredItem != null)
        {
            inventory.RemoveItem(Base.RequiredItem);
            yield return DialogManager.Instance.QueueDialogTextCoroutine($"Gave the {Base.RequiredItem.Name}");
        }

        if(Base.RewardItem != null)
        {
            string playerName = player.GetComponent<PlayerController>().Name;
            string rewardText = $"{playerName} received ";
            string plural = "";
            int amount = 1;
            if(Base.RewardItemCount > 1)
            {
                amount = Base.RewardItemCount;
                rewardText += Base.RewardItemCount.ToString() + " ";
                plural += "s";
            }
            while(amount > 0)
            {
                inventory.AddItem(Base.RewardItem);
                amount--;
            }
            rewardText += Base.RewardItem.Name + plural;

            //yield return DialogManager.Instance.ShowDialogText(rewardText);
            yield return DialogManager.Instance.QueueDialogTextCoroutine(rewardText);
        }

        var questList = QuestList.GetQuestList();
        questList.AddQuest(this);
    }

    public bool CanBeCompleted()
    {
        bool result = true;
        var inventory = Inventory.GetInventory();
        if(Base.RequiredItem != null)
        {
            if(!inventory.HasItem(Base.RequiredItem))
            {
                result = false;
            }
        }
        if(result && Base.RequiredFlags.Count > 0)
        {
            for(int i = 0; i < Base.RequiredFlags.Count; i++)
            {
                if(QuestFlags.Instance.GetFlag(Base.RequiredFlags[i]))
                {
                    continue;
                }
                else
                {
                    result = false;
                    break;
                }
            }
        }
        return result;
    }

    public void OnYesFunc()
    {
        GameController.Instance.state = thisState;
        result = true;
    }
    public void OnNoFunc()
    {
        GameController.Instance.state = thisState;
    }

    public bool ConfirmQuestResolve()
    {
        return result;
    }
}

[System.Serializable]
public class QuestSaveData
{
    public string name;
    public QuestStatus status;
}

public enum QuestStatus { None, Started, Completed }
