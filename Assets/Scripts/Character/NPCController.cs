using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class NPCController : MonoBehaviour, Interactable, ISavable
{
  [SerializeField] Dialog dialog;

  [Header("Quests")]
  [SerializeField] List<string> flagsToSetOnInteraction;
  [SerializeField] QuestBase questToStart;
  [SerializeField] QuestBase questToComplete;

  public List<string> FlagsToSetOnInteraction {
    //get { return flagsToSetOnInteraction; }
    set 
    {
      for(int i = 0; i < value.Count; i++)
      {
        flagsToSetOnInteraction.Add(value[i]);
      }
    }
  }

  public QuestBase QuestToStart {
    get { return questToStart; }
    set { questToStart = value; }
  }
  public QuestBase QuestToComplete {
    get { return questToComplete; }
    set { questToComplete = value; }
  }

  [Header("Movement")]
  [SerializeField] List<Vector2> movementPattern;
  [SerializeField] float timeBetweenPattern;

  [Header("State")]
  //[SerializeField] MyGameObjectEvent action;
  [SerializeField] UnityEvent action;

  NPCState state;
  float idleTimer;
  int currentPattern = 0;
  Quest activeQuest;

  Character character;
  ItemGiver itemGiver;
  MonGiver monGiver;
  StarterMenuOpener starterMenuOpener;
  ConditionalQuestSetter conditionalQuestSetter;

  private void Awake()
  {
    character = GetComponent<Character>();
    itemGiver = GetComponent<ItemGiver>();
    monGiver = GetComponent<MonGiver>();
    starterMenuOpener = GetComponent<StarterMenuOpener>();
    conditionalQuestSetter = GetComponent<ConditionalQuestSetter>();
    //flagsToSetOnInteraction = new List<string>();
  }

  public void SetFlags()
  {
    Debug.Log("Setting Flags in NPCController");
    foreach(string flag in flagsToSetOnInteraction)
    {
      QuestFlags.Instance.SetFlag(flag, true);
    }
    flagsToSetOnInteraction = new List<string>();
  }

  public IEnumerator Interact(Transform initiator)
  {
    if(state == NPCState.Idle)
    {
      SetFlags();

      if(conditionalQuestSetter != null)
      {
        conditionalQuestSetter.CheckCondition();
      }

      state = NPCState.Dialog;
      character.LookTowards(initiator.position);

      if(questToComplete != null)
      {
        var quest = new Quest(questToComplete);
        yield return quest.CompleteQuest(initiator);
        questToComplete = null;
      }

      if(itemGiver != null && itemGiver.CanBeGiven())
      {
        yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
      }
      if(starterMenuOpener != null && monGiver != null && !monGiver.Used)
      {
        //yield return DialogManager.Instance.ShowDialog(monGiver.Dialog);
        yield return DialogManager.Instance.QueueDialogCoroutine(monGiver.Dialog);
        GameController.Instance.StarterSelectMenu();
        yield return new WaitUntil(() => GameController.Instance.State != GameState.StarterSelectMenu);
      }
      //else if(monGiver != null && monGiver.CanBeGiven())
      if(monGiver != null && monGiver.CanBeGiven())
      {
        yield return monGiver.GiveMon(initiator.GetComponent<PlayerController>());
      }
      else if(questToStart != null)
      {
        activeQuest = new Quest(questToStart);
        yield return activeQuest.StartQuest();
        questToStart = null;

        //!check if quest has already been completed
        // if(activeQuest.CanBeCompleted())
        // {
        //   //!  appears below as well; consolidate?
        //   GameController.Instance.OpenConfirmationMenu(activeQuest.Base.ConfirmationMessage, activeQuest.OnYesFunc, activeQuest.OnNoFunc);
        //   yield return GameController.Instance.confirmationMenu.WaitForChoice();

        //   if(activeQuest.ConfirmQuestResolve())
        //   {
        //     yield return activeQuest.CompleteQuest(initiator);
        //     activeQuest = null;
        //   }
        // }
      }
      else if(activeQuest != null)
      {
        if(activeQuest.CanBeCompleted())
        {
          if(activeQuest.Base.PromptToComplete && activeQuest.Base.ConfirmationMessage != null)
          {
            GameController.Instance.OpenConfirmationMenu(activeQuest.Base.ConfirmationMessage, activeQuest.OnYesFunc, activeQuest.OnNoFunc);
            yield return GameController.Instance.confirmationMenu.WaitForChoice();

            if(activeQuest.ConfirmQuestResolve())
            {
              yield return activeQuest.CompleteQuest(initiator);
              activeQuest = null;
            }
          }
          else
          {
              yield return activeQuest.CompleteQuest(initiator);
              activeQuest = null;
          }
        }
        else
        {
          //yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialog);
          yield return DialogManager.Instance.QueueDialogCoroutine(activeQuest.Base.InProgressDialog);
        }
      }
      else
      {
        //yield return DialogManager.Instance.ShowDialog(dialog);
        yield return DialogManager.Instance.QueueDialogCoroutine(dialog);
        action?.Invoke();
      }

      idleTimer = 0f;
      state = NPCState.Idle;

    }
  }

  private void Update()
  {

    if(state == NPCState.Idle)
    {
      idleTimer += Time.deltaTime;
      if(idleTimer > timeBetweenPattern)
      {
        idleTimer = 0f;
        if(movementPattern.Count > 0)
        {
          StartCoroutine(Walk());
        }
      }
    }

    //character.HandleUpdate();
  }

  IEnumerator Walk()
  {
    state = NPCState.Walking;

    var oldPos = transform.position;

    yield return character.Move(movementPattern[currentPattern]);
    
    if(transform.position != oldPos)
    {
      currentPattern = (currentPattern + 1) % movementPattern.Count;
    }

    state = NPCState.Idle;
  }

  public object CaptureState()
  {
    var saveData = new NPCQuestSaveData();
    saveData.activeQuest = activeQuest?.GetSaveData();
    
    if(flagsToSetOnInteraction.Count > 0)
    {
      saveData.flagsToSet = flagsToSetOnInteraction;
    }
    else
    {
      saveData.flagsToSet = new List<string>();
    }
    //questToStart is a QuestBase, not a Quest, and therefore cannot be converted into QuestSaveData directly
    if(questToStart != null)
    {
      saveData.questToStart = (new Quest(questToStart)).GetSaveData();
    }
    //questToStart is a QuestBase, not a Quest, and therefore cannot be converted into QuestSaveData directly
    if(questToComplete != null)
    {
      saveData.questToComplete = (new Quest(questToComplete)).GetSaveData();
    }

    if(conditionalQuestSetter != null)
    {
      saveData.startDone = conditionalQuestSetter.StartDone;
      saveData.completeDone = conditionalQuestSetter.CompleteDone;
    }

    return saveData;
  }

  public void RestoreState(object state)
  {
    var saveData = state as NPCQuestSaveData;
    if(saveData != null)
    {
      flagsToSetOnInteraction = saveData.flagsToSet;
      
      activeQuest = (saveData.activeQuest != null)? new Quest(saveData.activeQuest) : null;

      questToStart = (saveData.questToStart != null)? new Quest(saveData.questToStart).Base : null;
      
      questToComplete = (saveData.questToComplete != null)? new Quest(saveData.questToComplete).Base : null;

      if(conditionalQuestSetter != null)
      {
        conditionalQuestSetter.StartDone = saveData.startDone;
        conditionalQuestSetter.CompleteDone = saveData.completeDone;
      }
    }
  }
}

[System.Serializable]
public class NPCQuestSaveData
{
  public QuestSaveData activeQuest;
  public QuestSaveData questToStart;
  public QuestSaveData questToComplete;
  public bool startDone;
  public bool completeDone;
  public List<string> flagsToSet;
}

public enum NPCState { Idle, Walking, Dialog }
