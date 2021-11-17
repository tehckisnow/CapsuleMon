using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver}

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    private int currentAction;
    private int currentMove;
    private int currentMember;

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
        enemyUnit.Setup(wildMon);

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Mon.Moves);

        yield return dialogBox.TypeDialog($"A wild {enemyUnit.Mon.Base.Name} appeared!");

        ChooseFirstTurn();
    }

    private void ChooseFirstTurn()
    {
        if(playerUnit.Mon.Speed >= enemyUnit.Mon.Speed)
        {
            ActionSelection();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    private void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Mons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    private void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    private void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Mons);
        partyScreen.gameObject.SetActive(true);
    }

    private void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;

        var move = playerUnit.Mon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        //if the battle state was not changed by RunMove, then go to next step
        if(state == BattleState.PerformMove)
        {
            StartCoroutine(EnemyMove());
        }

    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        var move = enemyUnit.Mon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        //if the battle state was not changed by RunMove, then go to next step
        if(state == BattleState.PerformMove)
        {
            ActionSelection();
        }
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Mon.OnBeforeMove();
        if(!canRunMove)
        {
            yield return ShowStatusChanges(sourceUnit.Mon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Mon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Mon.Base.Name} used {move.Base.Name}");
        
        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        targetUnit.PlayHitAnimation();

        if(move.Base.Category == MoveCategory.Status)
        {
            yield return RunMoveEffects(move, sourceUnit.Mon, targetUnit.Mon);
        }
        else
        {
            var damageDetails = targetUnit.Mon.TakeDamage(move, sourceUnit.Mon);
            yield return targetUnit.Hud.UpdateHP();
            yield return ShowDamageDetails(damageDetails);
        }

        if(targetUnit.Mon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{targetUnit.Mon.Base.Name} fainted");
            targetUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            
            CheckForBattleOver(targetUnit);
        }

        //statuses like burn or psn will hurt the mon after the turn
        sourceUnit.Mon.OnAfterTurn();
        yield return ShowStatusChanges(sourceUnit.Mon);
        yield return sourceUnit.Hud.UpdateHP();
        if(sourceUnit.Mon.HP <= 0)
        {
            yield return dialogBox.TypeDialog($"{sourceUnit.Mon.Base.Name} fainted");
            sourceUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);
            
            CheckForBattleOver(sourceUnit);
        }
    }

    IEnumerator RunMoveEffects(Move move, Mon source, Mon target)
    {
        var effects = move.Base.Effects;

        // Stat Boosting
        if(effects.Boosts != null)
        {
            if(move.Base.Target == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
            }
        }

        // Status Condition
        if(effects.Status != ConditionID.none)
        {
            target.SetStatus(effects.Status);
        }

        // VolatileStatus Condition
        if(effects.VolatileStatus != ConditionID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChanges(source);
        yield return ShowStatusChanges(target);
    }

    IEnumerator ShowStatusChanges(Mon mon)
    {
        while(mon.StatusChanges.Count > 0)
        {
            var message = mon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    private void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if(faintedUnit.IsPlayerUnit)
        {
            var nextMon = playerParty.GetHealthyMon();
            if(nextMon != null)
            {
                OpenPartyScreen();
            }
            else
            {
                BattleOver(false);
            }
        }
        else
        {
            BattleOver(true);
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
        if(state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if(state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if(state == BattleState.PartyScreen)
        {
            HandlePartySelection();
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
                MoveSelection();
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
            StartCoroutine(PlayerMove());
        }
        else if(Input.GetButtonDown("Cancel"))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    private void HandlePartySelection()
    {
        if(Input.GetButtonDown("Down"))
        {
            currentMember += 2;
        }
        else if(Input.GetButtonDown("Up"))
        {
            currentMember -= 2;
        }
        else if(Input.GetButtonDown("Right"))
        {
            ++currentMember;
        }
        else if(Input.GetButtonDown("Left"))
        {
            --currentMember;
        }

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Mons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if(Input.GetButtonDown("Submit"))
        {
            var selectedMember = playerParty.Mons[currentMember];
            if(selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't sent out a fainted mon");
                return;
            }
            if(selectedMember == playerUnit.Mon)
            {
                partyScreen.SetMessageText("You can't switch with the same mon");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchMon(selectedMember));
        }
        else if(Input.GetButtonDown("Cancel"))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchMon(Mon newMon)
    {
        bool currentMonFainted = true;
        if(playerUnit.Mon.HP > 0)
        {
            currentMonFainted = false;
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Mon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newMon);
        dialogBox.SetMoveNames(newMon.Moves);
        yield return dialogBox.TypeDialog($"Go {newMon.Base.Name}!");

        if(currentMonFainted)
        {
            ChooseFirstTurn();
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }
}
