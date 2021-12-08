using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MonInfoScreen : MonoBehaviour
{
    [SerializeField] BattleHud hud;
    [SerializeField] TextMeshProUGUI baseName;
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] Image frontSprite;
    [SerializeField] Transform statsBG;
    [SerializeField] Transform moves;

    private Mon mon;

    public void SetupMon(Mon mon)
    {
        this.mon = mon;
        baseName.text = mon.Base.Name;
        frontSprite.sprite = mon.Base.FrontSprite;
        descriptionText.text = mon.Base.Description;

        hud.SetData(mon);

        SetStatData();
        SetMoveData();
    }

    public void SetMoveData()
    {
        for(int i = 0; i < MonBase.MaxNumberOfMoves; i++)
        {
            var moveText = moves.GetChild(i).gameObject.GetComponent<TextMeshProUGUI>();
            moveText.text = "-";
            if(mon.Moves.Count > i)
            {
                moveText.text = mon.Moves[i].Base.Name;
                moveText.text += " PP: " + mon.Moves[i].PP + "/" + mon.Moves[i].Base.PP;
            }

            // if(mon.Moves[i] != null) //! ArgumentOutOfRangeException: Index was out of range
            // {
            //     moveText.text = mon.Moves[i].Base.Name;
            //     moveText.text += " PP: " + mon.Moves[i].PP + "/" + mon.Moves[i].Base.PP;
            // }
        }
    }

    public void SetStatData()
    {
        int[] statblock = new int[]
        {
            mon.MaxHp,
            mon.Attack,
            mon.Defense,
            mon.SpAttack,
            mon.SpDefense,
            mon.Speed
        };

        string[] statLabels = new string[]
        {
            "MaxHp: ",
            "Att: ",
            "Def: ",
            "SpAtt: ",
            "SpDef: ",
            "Spd: "
        };

        int i = 0;
        foreach(Transform statText in statsBG)
        {
            var text = statText.gameObject.GetComponent<TextMeshProUGUI>();
            text.text = statLabels[i] + statblock[i].ToString();
            i++;
        }
    }

    public void HandleUpdate()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            CloseInfoScreen();
        }
    }

    private void CloseInfoScreen()
    {
        hud.ClearData();
        gameObject.SetActive(false);
        GameController.Instance.state = GameState.PartyScreen;
    }
}
