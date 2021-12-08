using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    PartyMemberUI[] memberSlots;
    List<Mon> mons;
    MonParty party;

    private bool reorderMode = false;
    private Mon reorderSlotA;
    private Mon reorderSlotB;

    private int selection = 0;
    public Mon SelectedMember {
        get 
        {
            if(mons.Count < 1)
            {
                return null;
            }
            else
            {
                return mons[selection];
            };
        }
    }
    //=> mons[selection];

    //Party screen can be called from different states like ActionSelection, RunningTurn, AboutToUse
    public BattleState? CalledFrom {get; set; }

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
        party = MonParty.GetPlayerParty();
        SetPartyData();

        party.OnUpdated += SetPartyData;
    }

    public void SetPartyData()
    {
        mons = party.Mons;

        for(int i = 0; i < memberSlots.Length; i++)
        {
            if(i < mons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].Init(mons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateMemberSelection(selection);

        messageText.text = "Choose a mon";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for(int i = 0; i < mons.Count; i++)
        {
            if(i == selectedMember)
            {
                //memberSlots[i].SetSelected(true);
                if(memberSlots[i].ToMove)
                {
                    memberSlots[i].SetMoveAndSelected(true, true);
                }
                else
                {
                    memberSlots[i].SetMoveAndSelected(false, true);
                }
            }
            else
            {
                //memberSlots[i].SetSelected(false);
                if(memberSlots[i].ToMove)
                {
                    memberSlots[i].SetMoveAndSelected(true, false);
                }
                else
                {
                    memberSlots[i].SetMoveAndSelected(false, false);
                }
            }
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }

    public void HandleUpdate(Action onSelected, Action onBack)
    {
        var prevSelection = selection;

        if(Input.GetButtonDown("Down"))
        {
            selection += 2;
        }
        else if(Input.GetButtonDown("Up"))
        {
            selection -= 2;
        }
        else if(Input.GetButtonDown("Right"))
        {
            ++selection;
        }
        else if(Input.GetButtonDown("Left"))
        {
            --selection;
        }

        selection = Mathf.Clamp(selection, 0, mons.Count - 1);

        if(selection != prevSelection)
        {
            UpdateMemberSelection(selection);
        }

        if(Input.GetButtonDown("Submit"))
        {
            if(reorderMode)
            {
                reorderSlotB = SelectedMember;
                party.SwitchMons(reorderSlotA, reorderSlotB);
                ExitReorderMode();
            }
            else
            {
                onSelected?.Invoke();    
            }
        }
        else if(Input.GetButtonDown("Cancel"))
        {
            if(reorderMode)
            {
                ExitReorderMode();
            }
            else
            {
                onBack?.Invoke();    
            }
        }
    }

    public void ReorderMode()
    {
        reorderMode = true;
        GameController.Instance.state = GameState.PartyScreen;
        memberSlots[selection].SetToMove(true);
        memberSlots[selection].SetMoveAndSelected(true, true);
        reorderSlotA = SelectedMember;
    }

    public void ExitReorderMode()
    {
        reorderSlotA = null;
        reorderSlotB = null;
        foreach(PartyMemberUI partyMember in memberSlots)
        {
            partyMember.SetToMove();
        }
        reorderMode = false;
        UpdateMemberSelection(selection);
    }

    public void ShowIfTmIsUsable(TmItem tmItem)
    {
        for(int i = 0; i < mons.Count; i++)
        {
            string message = tmItem.CanBeTaught(mons[i])? "ABLE!" : "NOT ABLE!";
            memberSlots[i].SetMessage(message);
        }
    }

    public void ClearMemberSlotMessages()
    {
        for(int i = 0; i < mons.Count; i++)
        {
            memberSlots[i].SetMessage("");
        }
    }
}
