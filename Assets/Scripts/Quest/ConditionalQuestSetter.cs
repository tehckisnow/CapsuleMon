using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalQuestSetter : MonoBehaviour
{
    //! consider adding in support for questFlags!

    [SerializeField] NPCController npc;
    //if this quest has been started/finished
    [SerializeField] QuestBase ifThisQuestIs;
    [SerializeField] QuestStatus status = QuestStatus.Started;
    //then the START or COMPLETE quest, respectively,
    //will be replaced by;
    [SerializeField] QuestBase replaceStartQuestWith = null;
    [SerializeField] bool onlyDoStartOnce = false;
    [SerializeField] QuestBase replaceCompleteQuestWith = null;
    [SerializeField] bool onlyDoCompleteOnce = false;
    
    [SerializeField] bool addFlags;
    [SerializeField] List<string> questFlagsToAdd;
    // [SerializeField] bool removeFlags;
    // [SerializeField] List<string> questFlagsToRemove;

    private QuestList questList;

    private bool startDone = false;
    private bool completeDone = false;
    
    public bool StartDone {
        get { return startDone; }
        set { startDone = value; }
    }
    public bool CompleteDone {
        get { return completeDone; }
        set { completeDone = value; }
    }

    private void Awake()
    {
        questList = QuestList.GetQuestList();
    }

    private void Start()
    {
        CheckCondition();
    }

    public void CheckCondition()
    {
        if(status == QuestStatus.Started)
        {
            if(questList.IsStarted(ifThisQuestIs.Name.ToString()))
            {
                ReplaceQuest();
            }
        }
        else if(status == QuestStatus.Completed)
        {
            if(questList.IsCompleted(ifThisQuestIs.Name.ToString()))
            {
                ReplaceQuest();
            }
        }
    }

    private void ReplaceQuest()
    {
        if(replaceStartQuestWith != null && !startDone)
        {
            npc.QuestToStart = replaceStartQuestWith;
            if(onlyDoStartOnce)
            {
                startDone = true;
            }
        }
        if(replaceCompleteQuestWith != null && !completeDone)
        {
            npc.QuestToComplete = replaceCompleteQuestWith;
            if(onlyDoCompleteOnce)
            {
                completeDone = true;
            }
        }
        if(addFlags)
        {
            AddFlags();
        }
    }

    private void AddFlags()
    {
        npc.FlagsToSetOnInteraction = questFlagsToAdd;
        questFlagsToAdd = new List<string>();
    }
}
