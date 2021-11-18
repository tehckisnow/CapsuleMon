using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, PartyScreen, BattleOver}
public enum BattleAction { Move, SwitchMon, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;

    public event Action<bool> OnBattleOver;

    BattleState state;
    BattleState? prevState;
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

        ActionSelection();
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

    IEnumerator RunTurns(BattleAction playerAction)
    {
        state = BattleState.RunningTurn;
        if(playerAction == BattleAction.Move)
        {
            playerUnit.Mon.CurrentMove = playerUnit.Mon.Moves[currentMove];
            enemyUnit.Mon.CurrentMove = enemyUnit.Mon.GetRandomMove();

            //Check who goes first
            int playerMovePriority = playerUnit.Mon.CurrentMove.Base.Priority;
            int enemyMovePriority = enemyUnit.Mon.CurrentMove.Base.Priority;

            bool playerGoesFirst = true;
            if(enemyMovePriority > playerMovePriority)
            {
                playerGoesFirst = false;
            }
            else if(enemyMovePriority == playerMovePriority)
            {
                playerGoesFirst = playerUnit.Mon.Speed >= enemyUnit.Mon.Speed;
            }

            var firstUnit = (playerGoesFirst) ? playerUnit : enemyUnit;
            var secondUnit = (playerGoesFirst) ? enemyUnit : playerUnit;

            var secondMon = secondUnit.Mon;

            //first turn
            yield return RunMove(firstUnit, secondUnit, firstUnit.Mon.CurrentMove);
            yield return RunAfterTurn(firstUnit);
            if(state == BattleState.BattleOver) yield break;

            if(secondMon.HP > 0)
            {
                //second turn
                yield return RunMove(secondUnit, firstUnit, secondUnit.Mon.CurrentMove);
                yield return RunAfterTurn(secondUnit);
                if(state == BattleState.BattleOver) yield break;
            }
        }
        else
        {
            if(playerAction == BattleAction.SwitchMon)
            {
                var selectedMon = playerParty.Mons[currentMember];
                state = BattleState.Busy;
                yield return SwitchMon(selectedMon);
            }

            //enemy turn
            var enemyMove = enemyUnit.Mon.GetRandomMove();
            yield return RunMove(enemyUnit, playerUnit, enemyMove);
            yield return RunAfterTurn(enemyUnit);
            if(state == BattleState.BattleOver)
            {
                yield break;
            }

        }

        if(state != BattleState.BattleOver)
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
        
        //check if move hits
        if(CheckIfMoveHits(move, sourceUnit.Mon, targetUnit.Mon))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            targetUnit.PlayHitAnimation();

            if(move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Mon, targetUnit.Mon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Mon.TakeDamage(move, sourceUnit.Mon);
                yield return targetUnit.Hud.UpdateHP();
                yield return ShowDamageDetails(damageDetails);
            }

            if(move.Base.SecondaryEffects != null && move.Base.SecondaryEffects.Count > 0 && targetUnit.Mon.HP > 0)
            {
                foreach(var secondary in move.Base.SecondaryEffects)
                {
                    var rnd = UnityEngine.Random.Range(1, 101);
                    if(rnd <= secondary.Chance)
                    {
                        yield return RunMoveEffects(secondary, sourceUnit.Mon, targetUnit.Mon, secondary.Target); }
                }
            }

            if(targetUnit.Mon.HP <= 0)
            {
                yield return dialogBox.TypeDialog($"{targetUnit.Mon.Base.Name} fainted");
                targetUnit.PlayFaintAnimation();

                yield return new WaitForSeconds(2f);
                
                CheckForBattleOver(targetUnit);
            }
        }
        else
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            yield return dialogBox.TypeDialog($"{sourceUnit.Mon.Base.Name}'s attack missed");
        }


    }

    IEnumerator RunMoveEffects(MoveEffects effects, Mon source, Mon target, MoveTarget moveTarget)
    {
        // Stat Boosting
        if(effects.Boosts != null)
        {
            if(moveTarget == MoveTarget.Self)
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

    IEnumerator RunAfterTurn(BattleUnit sourceUnit)
    {
        if(state == BattleState.BattleOver)
        {
            yield break;
        }

        //wait until this lambda function is true
        yield return new WaitUntil(() => state == BattleState.RunningTurn);

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

    private bool CheckIfMoveHits(Move move, Mon source, Mon target)
    {
        if(move.Base.AlwaysHits)
        {
            return true;
        }

        float moveAccuracy = move.Base.Accuracy;

        int accuracy = source.StatBoosts[Stat.Accuracy];
        int evasion = target.StatBoosts[Stat.Evasion];

        var boostValues = new float[] { 1f, 4f / 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f };

        if(accuracy > 0)
        {
            moveAccuracy *= boostValues[accuracy];
        }
        else
        {
            moveAccuracy /= boostValues[-accuracy];
        }

        if(evasion > 0)
        {
            moveAccuracy /= boostValues[evasion];
        }
        else
        {
            moveAccuracy *= boostValues[-evasion];
        }

        return UnityEngine.Random.Range(1, 101) <= moveAccuracy;
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
                prevState = state;
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
            var move = playerUnit.Mon.Moves[currentMove];
            if(move.PP == 0)
            {
                return;
            }

            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(RunTurns(BattleAction.Move));
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

            if(prevState == BattleState.ActionSelection)
            {
                prevState = null;
                StartCoroutine(RunTurns(BattleAction.SwitchMon));
            }
            else
            {
                state = BattleState.Busy;
                StartCoroutine(SwitchMon(selectedMember));
            }
        }
        else if(Input.GetButtonDown("Cancel"))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchMon(Mon newMon)
    {
        if(playerUnit.Mon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Mon.Base.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newMon);
        dialogBox.SetMoveNames(newMon.Moves);
        yield return dialogBox.TypeDialog($"Go {newMon.Base.Name}!");

        state = BattleState.RunningTurn;
    }
}
