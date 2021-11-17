using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PartyScreen : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI messageText;

    PartyMemberUI[] memberSlots;

    public void Init()
    {
        memberSlots = GetComponentsInChildren<PartyMemberUI>();
    }

    public void SetPartyData(List<Mon> mons)
    {
        for(int i = 0; i < memberSlots.Length; i++)
        {
            if(i < mons.Count)
            {
                memberSlots[i].SetData(mons[i]);
            }
            else
            {
                memberSlots[i].gameObject.SetActive(false);
            }
        }
        messageText.text = "Choose a mon";
    }
}
