using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class MyGameObjectEvent : UnityEvent<GameObject>
{
}

public class NPCController : MonoBehaviour, Interactable, ISavable
{
  [SerializeField] Dialog dialog;

  [Header("Quests")]
  [SerializeField] QuestBase questToStart;
  [SerializeField] QuestBase questToComplete;

  [Header("Movement")]
  [SerializeField] List<Vector2> movementPattern;
  [SerializeField] float timeBetweenPattern;

  [Header("State")]
  [SerializeField] MyGameObjectEvent action;

  NPCState state;
  float idleTimer;
  int currentPattern = 0;
  Quest activeQuest;

  Character character;
  ItemGiver itemGiver;
  MonGiver monGiver;

  private void Awake()
  {
    character = GetComponent<Character>();
    itemGiver = GetComponent<ItemGiver>();
    monGiver = GetComponent<MonGiver>();
  }

  public IEnumerator Interact(Transform initiator)
  {
    if(state == NPCState.Idle)
    {
      state = NPCState.Dialog;
      character.LookTowards(initiator.position);

      if(questToComplete != null)
      {
        var quest = new Quest(questToComplete);
        yield return quest.CompleteQuest(initiator);
        questToComplete = null;

        Debug.Log($"{quest.Base.Name} completed");
      }

      if(itemGiver != null && itemGiver.CanBeGiven())
      {
        yield return itemGiver.GiveItem(initiator.GetComponent<PlayerController>());
      }
      else if(monGiver != null && monGiver.CanBeGiven())
      {
        yield return monGiver.GiveMon(initiator.GetComponent<PlayerController>());
      }
      else if(questToStart != null)
      {
        activeQuest = new Quest(questToStart);
        yield return activeQuest.StartQuest();
        questToStart = null;

        //check if quest has already been completed
        if(activeQuest.CanBeCompleted())
        {
          yield return activeQuest.CompleteQuest(initiator);
          activeQuest = null;
        }
      }
      else if(activeQuest != null)
      {
        if(activeQuest.CanBeCompleted())
        {
          yield return activeQuest.CompleteQuest(initiator);
          activeQuest = null;
        }
        else
        {
          yield return DialogManager.Instance.ShowDialog(activeQuest.Base.InProgressDialog);
        }
      }
      else
      {
        yield return DialogManager.Instance.ShowDialog(dialog);
      }

      idleTimer = 0f;
      state = NPCState.Idle;

      //StartCoroutine(WaitForDialogAndDoAction(action, initiator.gameObject));
    }
  }

  // IEnumerator WaitForDialogAndDoAction(MyGameObjectEvent unityEvent, GameObject initiatorObj)
  // {
  //   yield return DialogManager.Instance.ShowDialog(dialog, () => {
  //       idleTimer = 0f;
  //       state = NPCState.Idle;
  //     });
  //   yield return new WaitUntil(() => FindObjectOfType<GameController>().state != GameState.Dialog);
  //   unityEvent?.Invoke(initiatorObj);
  // }

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

    return saveData;
  }

  public void RestoreState(object state)
  {
    var saveData = state as NPCQuestSaveData;
    if(saveData != null)
    {
      activeQuest = (saveData.activeQuest != null)? new Quest(saveData.activeQuest) : null;

      questToStart = (saveData.questToStart != null)? new Quest(saveData.questToStart).Base : null;
      
      questToComplete = (saveData.questToComplete != null)? new Quest(saveData.questToComplete).Base : null;
    }
  }
}

[System.Serializable]
public class NPCQuestSaveData
{
  public QuestSaveData activeQuest;
  public QuestSaveData questToStart;
  public QuestSaveData questToComplete;
}

public enum NPCState { Idle, Walking, Dialog }
