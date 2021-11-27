using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { FreeRoam, Battle, Dialog, Cutscene, Menu, PartyScreen, Bag, StarterSelectMenu, Paused }

public class GameController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] Camera worldCamera;
    [SerializeField] SelectStarter selectStarter;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] InventoryUI inventoryUI;

    public GameState state;
    public GameState State => state;

    public GameState prevState;

    public SceneDetails CurrentScene { get; private set; }
    public SceneDetails PrevScene { get; private set; }

    private MenuController menuController;

    public static GameController Instance {get; private set; }

    private void Awake()
    {
        Instance = this;

        menuController = GetComponent<MenuController>();

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        MonDB.Init();
        MoveDB.Init();
        ConditionsDB.Init();
        ItemDB.Init();
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
        selectStarter.gameObject.SetActive(true);
        selectStarter.Init();
        state = GameState.StarterSelectMenu;
        //DialogManager.Instance.gameObject.SetActive(false);
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
    }

    private void Update()
    {
        if(state == GameState.FreeRoam)
        {
            playerController.HandleUpdate();

            if(Input.GetButtonDown("Cancel"))
            {
                menuController.OpenMenu();
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
        else if(state == GameState.Menu)
        {
            menuController.HandleUpdate();
        }
        else if(state == GameState.PartyScreen)
        {
            Action onSelected = () =>
            {
                //TODO: goto summary screen

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
            SavingSystem.i.Save("saveSlot1");
            state = GameState.FreeRoam;
        }
        else if(selectedItem == 3)
        {
            //load
            SavingSystem.i.Load("saveSlot1");
            state = GameState.FreeRoam;
        }

    }
}
