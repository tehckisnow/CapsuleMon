using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    PartyMemberUI[] memberSlots;
    List<Mon> mons;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>(true);
    }

    public void SetPartyData(List<Mon> mons)
    {
        this.mons = mons;
        for(int i = 0; i < memberSlots.Length; i++)
        {
            if(i < mons.Count)
            {
                memberSlots[i].gameObject.SetActive(true);
                memberSlots[i].SetData(mons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }
        messageText.text = "Choose a mon";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for(int i = 0; i < mons.Count; i++)
        {
            if(i == selectedMember)
            {
                memberSlots[i].SetSelected(true);
            }
            else
            {
                memberSlots[i].SetSelected(false);
            }
        }
    }

    public void SetMessageText(string message)
    {
        messageText.text = message;
    }
}
