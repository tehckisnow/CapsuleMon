using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SelectStarter : MonoBehaviour
{
    [SerializeField] GameObject selectStarterCanvas;
    [SerializeField] List<TextMeshProUGUI> labelsList;
    [SerializeField] List<MonBase> starterList;
    [SerializeField] Color highlightedColor;
    [SerializeField] List<Image> starterImages;
    
    private int currentSelection = 1;
    private Color unhighlightedColor;
    private MonParty playerParty;
    private GameObject playerObject;

    private void Awake()
    {
        unhighlightedColor = labelsList[0].color;
        Init();
    }

    public void Init()
    {
        for(int i = 0; i < starterList.Count; i++)
        {
            starterImages[i].sprite = starterList[i].FrontSprite;
            labelsList[i].text = starterList[i].Name;
        }
    }

    public void OpenSelectStarter(GameObject player)
    {
        FindObjectOfType<GameController>().StarterSelectMenu();
        playerParty = player.GetComponent<MonParty>();
    }

    public void HandleSelectStarter()
    {
        if(Input.GetButtonDown("Right"))
        {
            ++currentSelection;
        }
        else if(Input.GetButtonDown("Left"))
        {
            --currentSelection;
        }

        currentSelection = Mathf.Clamp(currentSelection, 0, labelsList.Count);

        UpdateStarterLabelsList(currentSelection);

        if(Input.GetButtonDown("Submit"))
        {
            ChooseStarter(currentSelection);
        }

        if(Input.GetButtonDown("Cancel"))
        {
            selectStarterCanvas.gameObject.SetActive(false);
            FindObjectOfType<GameController>().state = GameState.FreeRoam;
        }
    }

    private void UpdateStarterLabelsList(int selection)
    {
        for(int i = 0; i < labelsList.Count; i++)
        {
            if( i == selection)
            {
                labelsList[i].color = highlightedColor;
            }
            else
            {
                labelsList[i].color = unhighlightedColor;
            }
        }
    }

    private void ChooseStarter(int selection)
    {
        playerParty.Mons[0] = new Mon(starterList[selection], 5);
        //nickname?

        selectStarterCanvas.gameObject.SetActive(false);
        FindObjectOfType<GameController>().state = GameState.FreeRoam;
    }
}
