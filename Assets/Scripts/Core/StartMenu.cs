using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartMenu : MonoBehaviour
{
    [SerializeField] GlobalSO Global;
    [SerializeField] GameObject player;
    [SerializeField] LevelLoader levelLoader;
    [SerializeField] TextMeshProUGUI newText;
    [SerializeField] TextMeshProUGUI loadText;
    private string textFromLoadText;
    [SerializeField] TextMeshProUGUI settingsText;
    
    [SerializeField] Image fixedMenu;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI moneyText;

    [SerializeField] GameObject tempPersistentNameSetter;
    [SerializeField] GameObject tempPersistentGameLoader;

    private bool loadDisabled = false;

    private List<TextMeshProUGUI> options;
    private int currentSelection = 0;

    private void Awake()
    {
        textFromLoadText = loadText.text;
        options = new List<TextMeshProUGUI>() { newText, loadText, settingsText };
    }

    public void Open()
    {
        gameObject.SetActive(true);
        this.enabled = true;
        InitMenu();
    }

    public void Close()
    {
        FixedMenu();
        currentSelection = 0;
        gameObject.SetActive(false);
    }

    public void InitMenu()
    {
        //get save data
        var controller = player.GetComponent<PlayerController>();
        var savableEntity = player.GetComponent<SavableEntity>();
        savableEntity.SetUniqueId(Global.PlayerID);
        
        SavingSystem.i.Load(Global.SaveSlotName);
        SavingSystem.i.RestoreEntity(savableEntity);

        //if no savedata found, disable load
        if(controller.Name == "")
        {
            loadDisabled = true;
        }
        
        SetFixedMenuValues(controller.Name, controller.Money);
        UpdateItemSelection();
    }

    private void SetFixedMenuValues(string name, int money)
    {
        nameText.text = name;
        moneyText.text = "$" + money.ToString();
    }

    private void FixedMenu(bool val=false)
    {
        fixedMenu.gameObject.SetActive(val);
    }

    private void Update()
    {
        HandleUpdate();
    }

    public void HandleUpdate()
    {
        if(Input.GetButtonDown("Down"))
        {
            currentSelection++;
            
            //skip over load
            currentSelection = Mathf.Clamp(currentSelection, 0, options.Count - 1);
            if(loadDisabled && options[currentSelection].text == "Load")
            {
                currentSelection++;
            }
        }
        if(Input.GetButtonDown("Up"))
        {
            currentSelection--;

            //skip over load
            currentSelection = Mathf.Clamp(currentSelection, 0, options.Count - 1);
            if(loadDisabled && options[currentSelection].text == "Load")
            {
                currentSelection--;
            }
        }

        //Load can still be selected if it is first or last in list because it will be clamped to selection
        currentSelection = Mathf.Clamp(currentSelection, 0, options.Count - 1);

        UpdateItemSelection();

        if(Input.GetButtonDown("Submit"))
        {
            Submit();
        }
    }

    private void Submit()
    {
        var choice = options[currentSelection].text;
        switch(choice)
        {
            case "New":
                NewGame();
                break;
            case "Load":
                Load();
                break;
            case "Settings":
                Settings();
                break;
            default:
                break;
        }
    }

    public void UpdateItemSelection()
    {
        for(int i = 0; i < options.Count; i++)
        {
            if(i == currentSelection)
            {
                options[i].color = GlobalSettings.i.HighlightedColor;
                if(options[i].text == textFromLoadText)
                {
                    FixedMenu(true);
                }
                else
                {
                    FixedMenu();
                }
            }
            else
            {
                options[i].color = GlobalSettings.i.UnhighlightedColor;
            }
        }

        if(loadDisabled)
        {
            loadText.color = Color.gray;
        }
    }

    public void NewGame()
    {
        Instantiate(tempPersistentNameSetter);
    }

    public void Load()
    {
        if(!loadDisabled)
        {
            var tempComp = Instantiate(tempPersistentGameLoader).GetComponent<TempPersistentGameLoader>();
            tempComp.Init(Global.SaveSlotName);
        }
    }

    public void Settings()
    {
        levelLoader.LoadSettingsMenu();
        Close();
    }
}