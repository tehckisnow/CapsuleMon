using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleHud playerHud;

    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleHud enemyHud;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    private int currentAction;
    private int currentMove;

    MonParty playerParty;
    Mon wildMon;

    public void StartBattle(MonParty playerParty, Mon wildMon)
    {
        this.playerParty = playerParty;
        this.wildMon = wildMon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.Setup(playerParty.GetHealthyMon());
        playerHud.SetData(playerUnit.Mon);

        enemyUnit.Setup(wildMon);
        enemyHud.SetData(enemyUnit.Mon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Mon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Mon.Base.Name} appeared!");

        PlayerAction();
    }

    private void PlayerAction()
    {
        state = BattleState.PlayerAction;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    private void OpenPartyScreen()
    {
        partyScreen.SetPartyData(playerParty.Mons);
        partyScreen.gameObject.SetActive(true);
    }

    private void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;

        var move = playerUnit.Mon.Moves[currentMove];
        move.PP--;
        yield return dialogBox.TypeDialog($"{playerUnit.Mon.Base.Name} used {move.Base.Name}");
        
        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayHitAnimation();
        var damageDetails = enemyUnit.Mon.TakeDamage(move, playerUnit.Mon);
        yield return enemyHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if(damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{enemyUnit.Mon.Base.Name} fainted");
            enemyUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            OnBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Mon.GetRandomMove();
        move.PP--;
        yield return dialogBox.TypeDialog($"{enemyUnit.Mon.Base.Name} used {move.Base.Name}");
        
        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation();
        var damageDetails = playerUnit.Mon.TakeDamage(move, enemyUnit.Mon);
        yield return playerHud.UpdateHP();
        yield return ShowDamageDetails(damageDetails);

        if(damageDetails.Fainted)
        {
            yield return dialogBox.TypeDialog($"{playerUnit.Mon.Base.Name} fainted");
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            
            var nextMon = playerParty.GetHealthyMon();
            if(nextMon != null)
            {

                playerUnit.Setup(nextMon);
                playerHud.SetData(nextMon);

                dialogBox.SetMoveNames(nextMon.Moves);

                yield return dialogBox.TypeDialog($"Go {nextMon.Base.Name}!");

                PlayerAction();
            }
            else
            {
                OnBattleOver(false);
            }
        }
        else
        {
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if(damageDetails.Critical > 1f)
        {
            yield return dialogBox.TypeDialog("A critical hit!");
        }

        if(damageDetails.TypeEffectiveness > 1f)
        {
            yield return dialogBox.TypeDialog("It's super effective!");
        }
        else if(damageDetails.TypeEffectiveness < 1f)
        {
            yield return dialogBox.TypeDialog("It's not very effective...");
        }
    }

    public void HandleUpdate()
    {
        if(state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if(state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }

    private void HandleActionSelection()
    {
        if(Input.GetButtonDown("Down"))
        {
            currentAction += 2;
        }
        else if(Input.GetButtonDown("Up"))
        {
            currentAction -= 2;
        }
        else if(Input.GetButtonDown("Right"))
        {
            ++currentAction;
        }
        else if(Input.GetButtonDown("Left"))
        {
            --currentAction;
        }

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if(Input.GetButtonDown("Submit"))
        {
            if(currentAction == 0)
            {
                //Fight
                PlayerMove();
            }
            else if(currentAction == 1)
            {
                //bag

            }
            else if(currentAction == 2)
            {
                //Mon
                OpenPartyScreen();
            }
            else if(currentAction == 3)
            {
                //Run

            }
        }
    }

    private void HandleMoveSelection()
    {
        if(Input.GetButtonDown("Down"))
        {
            currentMove += 2;
        }
        else if(Input.GetButtonDown("Up"))
        {
            currentMove -= 2;
        }
        else if(Input.GetButtonDown("Right"))
        {
            ++currentMove;
        }
        else if(Input.GetButtonDown("Left"))
        {
            --currentMove;
        }

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Mon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Mon.Moves[currentMove]);

        if(Input.GetButtonDown("Submit"))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
        else if(Input.GetButtonDown("Cancel"))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            PlayerAction();
        }
    }
}
