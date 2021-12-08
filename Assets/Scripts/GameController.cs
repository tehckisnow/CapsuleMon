using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle, Dialog, Cutscene, Menu, PartyScreen, Bag, StarterSelectMenu, Paused, Evolution, NameSetter, MonInfoScreen, ConfirmationMenu, OptionsMenu, SetMonNick }

public class GameController : MonoBehaviour
{
    [SerializeField] GlobalSO Global;
    [SerializeField] Canvas uiCanvas;
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] NameSetterMenu nameSetterMenu;
    [SerializeField] MonInfoScreen monInfoScreen;
    [SerializeField] public ConfirmationMenu confirmationMenu;
    [SerializeField] NicknameMenu nicknameMenu;
    [SerializeField] GameObject optionsMenuPrefab;
    public OptionsMenu activeOptionsMenu;

    //[SerializeField] SelectStarter selectStarter;
    [SerializeField] GameObject selectStarterPrefab;
    private SelectStarter selectStarter;

    [Header("stuff")]
    [SerializeField] TextMeshProUGUI moneyDisplay;
    [SerializeField] TextMeshProUGUI nameDisplay;

    public GameState state;
    public GameState State => state;

    public GameState prevState;
    public GameState stateBeforeEvolution;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    private MenuController menuController;

    public static GameController Instance {get; private set; }

    private string saveSlotName;

    private void Awake()
    {
        saveSlotName = Global.SaveSlotName;
        Instance = this;

        menuController = GetComponent<MenuController>();

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        MonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
        QuestDB.Init();
    }

    private void Start()
    {
        battleSystem.OnBattleOver += EndBattle;

        partyScreen.Init();

        DialogManager.Instance.OnShowDialog += () =>
        {
            prevState = state;
            state = GameState.Dialog;
        };
        DialogManager.Instance.OnDialogFinished += () =>
        {
            if(state == GameState.Dialog)
            {
                state = prevState;
            }
        };

        menuController.onBack += () =>
        {
            state = GameState.FreeRoam;
        };

        menuController.onMenuSelected += OnMenuSelected;

        EvolutionManager.i.OnStartEvolution += () => 
        {
            stateBeforeEvolution = state;
            state = GameState.Evolution;
        };
        EvolutionManager.i.OnCompleteEvolution += () => 
        {
            state = stateBeforeEvolution;
        };

        UpdateMoneyDisplay();
        nameDisplay.text = PlayerController.Instance.Name;
    }

    public void UpdateMoneyDisplay()
    {
        //moneyDisplay.text = "$" + amount.ToString();
        moneyDisplay.text = "$" + PlayerController.Instance.Money.ToString();
    }

    public void UpdateNameDisplay()
    {
        nameDisplay.text = PlayerController.Instance.Name;
    }

    public void PauseGame(bool pause)
    {
        if(pause)
        {
            prevState = state;
            state = GameState.Paused;
        }
        else
        {
            state = prevState;
        }
    }

    public void StartBattle()
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        var playerParty = playerController.GetComponent<MonParty>();
        var wildMon = CurrentScene.GetComponent<MapArea>().GetRandomWildMon();
        var wildMonCopy = new Mon(wildMon.Base, wildMon.Level);

        battleSystem.StartBattle(playerParty, wildMonCopy);
    }

    TrainerController trainer;

    public void StartTrainerBattle(TrainerController trainer)
    {
        state = GameState.Battle;
        battleSystem.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);

        this.trainer = trainer;
        var playerParty = playerController.GetComponent<MonParty>();
        var trainerParty = trainer.GetComponent<MonParty>();

        battleSystem.StartTrainerBattle(playerParty, trainerParty);
    }

    public void StarterSelectMenu()
    {
        selectStarter = Instantiate(selectStarterPrefab).GetComponent<SelectStarter>();
        selectStarter.UpdatePrevState(state);
        state = GameState.StarterSelectMenu;
    }

    public void StartNameSetterMenu()
    {
        nameSetterMenu.gameObject.SetActive(true);
        state = GameState.NameSetter;
    }

    public void OpenMonInfoScreen()
    {
        monInfoScreen.gameObject.SetActive(true);
        state = GameState.MonInfoScreen;
        monInfoScreen.SetupMon(partyScreen.SelectedMember);
    }

    public void OpenConfirmationMenu(string message=null, Action yesAction=null, Action noAction=null, CancelAction cancelAction=CancelAction.ChooseNo)
    {
        confirmationMenu.OpenMenu(message, yesAction, noAction, cancelAction);
    }

    public void OpenOptionsMenu(List<string> texts, List<Action> actions)
    {
        activeOptionsMenu = Instantiate(optionsMenuPrefab, uiCanvas.gameObject.transform).GetComponent<OptionsMenu>();
        activeOptionsMenu.Open(texts, actions);
    }

    //Overflow to specify the gamestate to return to after setting nick
    public void OpenNicknameMenu(Mon mon, GameState gameState)
    {
        GameState defaultState = gameState;
        string message = $"Would you like to give a nickname to {mon.Base.Name}?";
        Action yesAction = () =>
        {
            nicknameMenu.gameObject.SetActive(true);
            nicknameMenu.Open(mon, defaultState);
        };
        Action noAction = ()=> { state = defaultState; };
        OpenConfirmationMenu(message, yesAction, noAction);
    }
    
    public void OpenNicknameMenu(Mon mon)
    {
        string message = $"Would you like to give a nickname to {mon.Base.Name}?";
        Action yesAction = () =>
        {
            nicknameMenu.gameObject.SetActive(true);
            nicknameMenu.Open(mon);
        };
        OpenConfirmationMenu(message, yesAction);

        //nicknameMenu.gameObject.SetActive(true);
        //nicknameMenu.Open(mon);
    }

    public void StartCoroutineFromNonMonoBehavior(IEnumerator ienumerator)
    {
        StartCoroutine(ienumerator);
    }

    public void OnEnterTrainersView(TrainerController trainer)
    {
        state = GameState.Cutscene;
        StartCoroutine(trainer.TriggerTrainerBattle(playerController));
    }

    private void EndBattle(bool won)
    {
        if(trainer != null && won == true)
        {
            trainer.BattleLost();
            trainer = null;
        }

        state = GameState.FreeRoam;
        battleSystem.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);

        if(!won)
        {
            //! white out
            var warpController = PlayerController.Instance.GetComponent<WarpController>();
            //warpController.GoToLastWarp();
            StartCoroutine(warpController.GoToLastWarpAnim());
        }
        else
        {
            var playerParty = playerController.GetComponent<MonParty>();
            StartCoroutine(playerParty.CheckForEvolutions());
        }
    }

    private void Update()
    {
        if(state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            if(Input.GetButtonDown("Cancel"))
            {
                menuController.OpenMenu();
                UpdateMoneyDisplay();
                state = GameState.Menu;
            }

        }
        else if(state == GameState.Battle)
        {
            battleSystem.HandleUpdate();
        }
        else if(state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate();
        }
        else if(state == GameState.StarterSelectMenu)
        {
            selectStarter.HandleSelectStarter();
        }
        else if(state == GameState.NameSetter)
        {
            nameSetterMenu.HandleUpdate();
        }
        else if(state == GameState.MonInfoScreen)
        {
            monInfoScreen.HandleUpdate();
        }
        else if(state == GameState.ConfirmationMenu)
        {
            confirmationMenu.HandleUpdate();
        }
        else if(state == GameState.OptionsMenu)
        {
            activeOptionsMenu.HandleUpdate();
        }
        else if(state == GameState.SetMonNick)
        {
            nicknameMenu.HandleUpdate();
        }
        else if(state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if(state == GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                //Summary screen
                if(partyScreen.SelectedMember == null)
                {
                    return;
                }

                //OpenMonInfoScreen();
                //!
                List<string> optionText = new List<string>() {"Info", "Reorder", "Release"};
                List<Action> optionAction = new List<Action>()
                {
                    () => { OpenMonInfoScreen(); },
                    () => { partyScreen.ReorderMode(); },
                    () => { StartCoroutine(ReleaseMonCheck()); }
                };
                OpenOptionsMenu(optionText, optionAction);
            };

            Action onBack = () =>
            {
                partyScreen.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            partyScreen.HandleUpdate(onSelected, onBack);
        }
        else if(state == GameState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            inventoryUI.HandleUpdate(onBack);
        }
    }

    Mon monToRelease;
    public IEnumerator ReleaseMon()
    {
        var mon = monToRelease;
        MonParty.GetPlayerParty().RemoveMon(mon);
        yield return DialogManager.Instance.ShowDialogText($"{mon.Name} has been released.");
        state = GameState.PartyScreen;
    }

    public IEnumerator ReleaseMonCheck()
    {
        var mon = partyScreen.SelectedMember;
        string message = $"Are you sure you want to release {mon.Name}?";
        monToRelease = mon;
        // Action releaseAction = () =>
        // {
        //     MonParty.GetPlayerParty().RemoveMon(mon);
        //     DialogManager.Instance.ShowDialogText($"{mon.Name} has been released.");
        //     state = GameState.PartyScreen;
        // };
        Action releaseAction = () => { StartCoroutine(ReleaseMon()); };
        Action declineAction = () => 
        {
            Debug.Log("no");
            state = GameState.PartyScreen;
        };
        OpenConfirmationMenu(message, releaseAction, declineAction);
        yield return null;
    }

    public void SetCurrentScene(SceneDetails currScene)
    {
        PrevScene = CurrentScene;
        CurrentScene = currScene;
    }

    private void OnMenuSelected(int selectedItem)
    {
        if(selectedItem == 0)
        {
            //mon
            partyScreen.gameObject.SetActive(true);
            state = GameState.PartyScreen;
        }
        else if(selectedItem == 1)
        {
            //bag
            inventoryUI.gameObject.SetActive(true);
            state = GameState.Bag;
        }
        else if(selectedItem == 2)
        {
            //save
            SavingSystem.i.Save(saveSlotName);
            state = GameState.FreeRoam;
        }
        else if(selectedItem == 3)
        {
            //load
            SavingSystem.i.Load(saveSlotName);
            state = GameState.FreeRoam;
        }
        else if(selectedItem == 4)
        {
            //quit
            Destroy(FindObjectOfType<EssentialObjects>().gameObject);
            SceneManager.LoadScene("Opening");
        }
    }
}
