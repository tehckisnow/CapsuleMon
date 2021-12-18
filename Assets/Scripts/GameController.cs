using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameState { FreeRoam, Battle, Dialog, Cutscene, Menu, PartyScreen, Bag, StarterSelectMenu, Paused, Evolution, NameSetter, MonInfoScreen, ConfirmationMenu, OptionsMenu, SetMonNick, MonStorage, MoveSelectionUI, ChooseMonToNickname, Shop }

public class GameController : MonoBehaviour
{
    [SerializeField] GlobalSO Global;
    [SerializeField] Canvas uiCanvas;
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] PartyScreen partyScreen;
    public PartyScreen PartyScreen => partyScreen;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] NameSetterMenu nameSetterMenu;
    [SerializeField] MonInfoScreen monInfoScreen;
    [SerializeField] public ConfirmationMenu confirmationMenu;
    [SerializeField] NicknameMenu nicknameMenu;
    [SerializeField] GameObject optionsMenuPrefab;
    public OptionsMenu activeOptionsMenu;
    [SerializeField] MonStorageUI monStorage;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] ShopUI shopUI;

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
            partyScreen.SetPartyData();
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

    public void OpenMonStorage()
    {
        monStorage.Open();
    }

    public void OpenMoveSelectionUI(Mon mon, MoveBase newMove)
    {
        //set state
        var localPrevState = state;
        state = GameState.MoveSelectionUI;
        //convert to List<MoveBase>
        var moves = new List<MoveBase>();
        foreach(Move move in mon.Moves)
        {
            moves.Add(move.Base);
        }
        //create onSelection action
        Action<int> onSelection = (int val) =>
        {
            Action revertState = default;
            revertState = RevertState;
            void RevertState()
            {
                DialogManager.Instance.OnDialogFinished -= revertState;
                state = localPrevState;
            }

            DialogManager.Instance.OnDialogFinished += revertState;
            moveSelectionUI.Close();

            if(val == mon.Moves.Count)
            {
                StartCoroutine(DialogManager.Instance.ShowDialogText($"{mon.Name} did not learn {newMove.Name}"));
            }
            else
            {
                Move moveToRemove = mon.Moves[val];
                mon.Moves.Remove(moveToRemove);
                mon.LearnMove(newMove);
                StartCoroutine(DialogManager.Instance.ShowDialogText($"{mon.Name} forgot {moveToRemove.Base.Name} and learned {newMove.Name}"));
            }
            // wait for dialog to close, then mon.SetReadyForMove();
            StartCoroutine(WhenDialogClose(() => mon.SetReadyForMove()));
        };

        Action onCancel = () =>
        {
            moveSelectionUI.GenericCancel();
            StartCoroutine(WhenDialogClose(() => mon.SetReadyForMove()));
        };

        //open moveSelection
        moveSelectionUI.gameObject.SetActive(true);
        // set onSelection action
        moveSelectionUI.SetOnSelectionAction(onSelection);
        // set onCancel action
        moveSelectionUI.SetOnCancelAction(onCancel);
        // send move data
        moveSelectionUI.SetMoveData(moves, newMove);
    }

    public void OpenShopUI(Shop shop)
    {
        shopUI.Open(shop);
    }

    public IEnumerator WhenDialogClose(Action action)
    {
        yield return null;
        Action onClose = action;
        Action unSub = default;
        unSub = Unsubscribe;
        void Unsubscribe()
        {
            DialogManager.Instance.OnDialogFinished -= onClose;
            DialogManager.Instance.OnDialogFinished -= unSub;
        }
        DialogManager.Instance.OnDialogFinished += onClose;
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

        partyScreen.SetPartyData();

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
            //!StartCoroutine(playerParty.CheckForEvolutions());
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
        else if(state == GameState.MonStorage)
        {
            monStorage.HandleUpdate();
        }
        else if(state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if(state == GameState.MoveSelectionUI)
        {
            moveSelectionUI.HandleMoveSelection();
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
                
                Action SendMonToStorage = () =>
                {
                    void RevertState()
                    {
                        DialogManager.Instance.OnDialogFinished -= RevertState;
                        state = GameState.PartyScreen;
                    }
                    GameController.Instance.state = GameState.Dialog;
                    DialogManager.Instance.OnDialogFinished += RevertState;
                    MonParty.GetPlayerParty().SendToStorage(partyScreen.SelectedMember);
                };
                List<string> optionText = new List<string>() {"Info", "Reorder", "Send to PC", "Release"};
                List<Action> optionAction = new List<Action>()
                {
                    () => { OpenMonInfoScreen(); },
                    () => { partyScreen.ReorderMode(); },
                    () => { MonParty.GetPlayerParty().OpenDepositConfirmation(partyScreen.SelectedMember); },
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
        else if(state == GameState.ChooseMonToNickname)
        {
            Action OnSelected = () =>
            {
                Mon selectedMon = partyScreen.SelectedMember;
                partyScreen.gameObject.SetActive(false);
                nicknameMenu.gameObject.SetActive(true);
                nicknameMenu.Open(selectedMon, GameState.FreeRoam);
            };
            Action OnBack = () =>
            {
                partyScreen.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };

            partyScreen.HandleUpdate(OnSelected, OnBack);
        }
        else if(state == GameState.Shop)
        {
            shopUI.HandleUpdate();
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
        if(MonParty.GetPlayerParty().LastMon())
        {
            RevertFromDialogTo(GameState.PartyScreen);
            yield break;
        }
        
        var mon = partyScreen.SelectedMember;
        string message = $"Are you sure you want to release {mon.Name}?";
        monToRelease = mon;
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

    public void RevertFromDialogTo(GameState newState)
    {
        void RevertState()
        {
            DialogManager.Instance.OnDialogFinished -= RevertState;
            state = newState;
        }
        GameController.Instance.state = GameState.Dialog;
        DialogManager.Instance.OnDialogFinished += RevertState;
    }

    public IEnumerator WaitUntilState(GameState gameState, bool isTrue=true)
    {
        if(isTrue)
        {
            yield return new WaitUntil(() => state == gameState);
        }
        else
        {
            yield return new WaitUntil(() => state != gameState);
        }
    }

    public IEnumerator WaitAndRevertState(float time, GameState gameState)
    {
        yield return new WaitForSeconds(time);
        GameController.Instance.state = gameState;
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
