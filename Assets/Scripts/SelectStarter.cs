using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

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

    private GameState prevState;

    private void Awake()
    {
        unhighlightedColor = labelsList[0].color;
        Init();
    }

    public void Init()
    {
        //playerParty = MonParty.GetPlayerParty();
        for(int i = 0; i < starterList.Count; i++)
        {
            starterImages[i].sprite = starterList[i].FrontSprite;
            labelsList[i].text = starterList[i].Name;
        }
    }

    private void Update()
    {
        var gameState = GameController.Instance.state;
        if(gameState != GameState.StarterSelectMenu)
        {
            gameState = GameState.StarterSelectMenu;
            Debug.Log(gameState);
        }
    }

    public void UpdatePrevState(GameState state)
    {
        prevState = state;
    }

    public void OpenSelectStarter()
    {
        GameController.Instance.StarterSelectMenu();
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
            CloseStarterMenu();
        }
    }

    private void UpdateStarterLabelsList(int selection)
    {
        for(int i = 0; i < labelsList.Count; i++)
        {
            if( i == selection)
            {
                labelsList[i].color = GlobalSettings.i.HighlightedColor;
            }
            else
            {
                labelsList[i].color = unhighlightedColor;
            }
        }
    }

    private void ChooseStarter(int selection)
    {
        //playerParty.Mons[0] = new Mon(starterList[selection], 5);
        MonBase starterBase = starterList[selection];

        //! find prof and set his mon to this
        var monGivers = FindObjectsOfType<MonGiver>();
        var prof = monGivers.FirstOrDefault(x => x.name == "Prof");
        //! how to ensure this is the prof?

        Mon mon = new Mon(starterBase, 5);
        prof.SetMonToGive(mon);

        CloseStarterMenu();
    }

    public void CloseStarterMenu()
    {
        //FindObjectOfType<GameController>().state = GameState.FreeRoam;
        GameController.Instance.state = prevState;
        //! prevState = ?? //can't be null

        //selectStarterCanvas.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }
}
