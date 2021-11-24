using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] int lettersPerSecond = 30;

    Color highlightedColor;
    Color unhighlightedColor;
    Color outOfPPColor = Color.red;

    [SerializeField] TextMeshProUGUI dialogText;
    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;
    [SerializeField] GameObject choiceBox;

    [SerializeField] List<TextMeshProUGUI> actionTexts;
    [SerializeField] List<TextMeshProUGUI> moveTexts;

    [SerializeField] TextMeshProUGUI ppText;
    [SerializeField] TextMeshProUGUI typeText;

    [SerializeField] TextMeshProUGUI yesText;
    [SerializeField] TextMeshProUGUI noText;

    void Start()
    {
        highlightedColor = GlobalSettings.i.HighlightedColor;
        unhighlightedColor = actionTexts[0].color;
    }

    public void SetDialog(string text)
    {
        dialogText.text = text;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";
        foreach(var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f/lettersPerSecond);
        }

        yield return new WaitForSeconds(1f);
    }

    public void EnableDialogText(bool enabled)
    {
        //dialogText.enabled = enabled;
        if(!enabled)
        {
            dialogText.color = Color.clear;
        }
        else
        {
            dialogText.color = unhighlightedColor;
        }
    }

    public void EnableActionSelector(bool enabled)
    {
        actionSelector.SetActive(enabled);
    }

    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void EnableChoiceBox(bool enabled)
    {
        choiceBox.SetActive(enabled);
    }

    public void UpdateActionSelection(int selectedAction)
    {
        for(int i = 0; i < actionTexts.Count; ++i)
        {
            if(i == selectedAction)
            {
                actionTexts[i].color = highlightedColor;
            }
            else
            {
                actionTexts[i].color = unhighlightedColor;
            }
        }
    }

    public void UpdateMoveSelection(int selectedMove, Move move)
    {
        for(int i = 0; i < moveTexts.Count; ++i)
        {
            if(i == selectedMove)
            {
                moveTexts[i].color = highlightedColor;
            }
            else
            {
                moveTexts[i].color = unhighlightedColor;
            }
        }

        ppText.text = $"PP {move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();

        if(move.PP == 0)
        {
            ppText.color = outOfPPColor;
        }
        else
        {
            ppText.color = unhighlightedColor;
        }
    }

    public void SetMoveNames(List<Move> moves)
    {
        for(int i = 0; i < moveTexts.Count; ++i)
        {
            if(i < moves.Count)
            {
                moveTexts[i].text = moves[i].Base.Name;
            }
            else
            {
                moveTexts[i].text = "-";
            }
        }
    }

    public void UpdateChoiceBox(bool yesSelected)
    {
        if(yesSelected)
        {
            yesText.color = highlightedColor;
            noText.color = unhighlightedColor;
        }
        else
        {
            yesText.color = unhighlightedColor;
            noText.color = highlightedColor;
        }
    }
}
