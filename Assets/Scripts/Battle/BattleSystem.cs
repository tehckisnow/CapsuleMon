using System;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum BattleState { Start, ActionSelection, MoveSelection, RunningTurn, Busy, Bag, PartyScreen, AboutToUse, MoveToForget, BattleOver}
public enum BattleAction { Move, SwitchMon, UseItem, Run }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] BattleUnit playerUnit;
    [SerializeField] BattleUnit enemyUnit;
    [SerializeField] BattleDialogBox dialogBox;
    [SerializeField] PartyScreen partyScreen;
    [SerializeField] Image playerImage;
    [SerializeField] Image trainerImage;
    [SerializeField] GameObject capsuleSprite;
    [SerializeField] MoveSelectionUI moveSelectionUI;
    [SerializeField] InventoryUI inventoryUI;

    public event Action<bool> OnBattleOver;

    BattleState state;
    
    private int currentAction;
    private int currentMove;
    private bool aboutToUseChoice = true;

    private AnimatedImage playerAnimatedImage;
    private AnimatedImage trainerAnimatedImage;

    private bool animPlaying = false;

    MonParty playerParty;
    MonParty trainerParty;
    Mon wildMon;

    private bool isTrainerBattle = false;
    PlayerController player;
    TrainerController trainer;

    private int escapeAttempts;
    private MoveBase moveToLearn;

    public void StartBattle(MonParty playerParty, Mon wildMon)
    {
        this.playerParty = playerParty;
        this.wildMon = wildMon;

        isTrainerBattle = false;
        player = playerParty.GetComponent<PlayerController>();

        StartCoroutine(SetupBattle());
    }

    public void StartTrainerBattle(MonParty playerParty, MonParty trainerParty)
    {
        this.playerParty = playerParty;
        this.trainerParty = trainerParty;
        
        isTrainerBattle = true;
        player = playerParty.GetComponent<PlayerController>();
        trainer = trainerParty.GetComponent<TrainerController>();
        
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        dialogBox.EnableMoveSelector(false);
        dialogBox.EnableActionSelector(false);
        playerUnit.Clear();
        enemyUnit.Clear();

        if(!isTrainerBattle)
        {
            //wild mon battle
            playerUnit.Setup(playerParty.GetHealthyMon());
            enemyUnit.Setup(wildMon);
            
            trainerImage.gameObject.SetActive(false);

            dialogBox.SetMoveNames(playerUnit.Mon.Moves);
            yield return dialogBox.TypeDialog($"A wild {enemyUnit.Mon.Name} appeared!");
        }
        else
        {

            playerUnit.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(false);

            playerImage.gameObject.SetActive(true);
            trainerImage.gameObject.SetActive(true);
            
            playerImage.sprite = player.Sprite;
            trainerImage.sprite = trainer.Sprite;

            playerAnimatedImage = playerImage.gameObject.GetComponent<AnimatedImage>();
            trainerAnimatedImage = trainerImage.gameObject.GetComponent<AnimatedImage>();
            
            dialogBox.SetDialog("");

            yield return WantsToBattleAnim();
            
            yield return new WaitUntil(() => !animPlaying);

            yield return dialogBox.TypeDialog($"{trainer.Name} wants to battle");

            yield return ReadyBattleAnim();

            //send out first mon of trainer
            // trainerImage.gameObject.SetActive(false);
            enemyUnit.gameObject.SetActive(true);
            var enemyMon = trainerParty.GetHealthyMon();
            enemyUnit.Setup(enemyMon);
            yield return dialogBox.TypeDialog($"{trainer.Name} sent out {enemyMon.Name}");

            //send out first mon of player
            //playerImage.gameObject.SetActive(false);
            playerUnit.gameObject.SetActive(true);
            var playerMon = playerParty.GetHealthyMon();
            playerUnit.Setup(playerMon);
            yield return dialogBox.TypeDialog($"Go {playerMon.Name}");
            dialogBox.SetMoveNames(playerUnit.Mon.Moves);
        }

        escapeAttempts = 0;
        partyScreen.Init();
        ActionSelection();
    }

    IEnumerator WantsToBattleAnim()
    {
        Action finishAction = () => { animPlaying = false; };
        animPlaying = true;
        trainerAnimatedImage.transform.position = new Vector3(400, 0);
        playerAnimatedImage.ReturnToOriginalPos(-400, 0, 1.5f);
        yield return trainerAnimatedImage.ReturnToOriginalPosCoroutine(400, 0, 1.5f, finishAction);
    }

    IEnumerator ReadyBattleAnim()
    {
        yield return new WaitForSeconds(0.5f);
        playerAnimatedImage.MoveRelativeLocal(-400, 0, 1.5f);
        trainerAnimatedImage.MoveRelativeLocal(400, 0, 1.5f);
        
        yield return new WaitForSeconds(1f);
        playerAnimatedImage.Disable();
        trainerAnimatedImage.Disable();
    }

    private void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Mons.ForEach(p => p.OnBattleOver());
        playerUnit.Hud.ClearData();
        enemyUnit.Hud.ClearData();
        OnBattleOver(won);
    }

    private void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("Choose an action");
        dialogBox.EnableActionSelector(true);
    }

    private void OpenBag()
    {
        state = BattleState.Bag;
        inventoryUI.gameObject.SetActive(true);
    }

    private void OpenPartyScreen()
    {
        partyScreen.CalledFrom = state;
        state = BattleState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
    }

    private void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    //state when enemy trainer's mon fainted and they are about to switch
    IEnumerator AboutToUse(Mon newMon)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"{trainer.Name} is about to use {newMon.Name}. Do you want to switch mon?");
        state = BattleState.AboutToUse;
        dialogBox.EnableChoiceBox(true);
    }

    IEnumerator ChooseMoveToForget(Mon mon, MoveBase newMove)
    {
        state = BattleState.Busy;
        yield return dialogBox.TypeDialog($"Choose a move you want to forget");
        moveSelectionUI.gameObject.SetActive(true);
        moveSelectionUI.SetMoveData(mon.Moves.Select(x => x.Base).ToList(), newMove);
        moveToLearn = newMove;

        state = BattleState.MoveToForget;
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
                var selectedMon = partyScreen.SelectedMember;
                state = BattleState.Busy;
                yield return SwitchMon(selectedMon);
            }
            else if(playerAction == BattleAction.UseItem)
            {
                //this is handled from the item screen, so do nothing and skip to enemy move
                dialogBox.EnableActionSelector(false);
            }
            else if(playerAction == BattleAction.Run)
            {
                yield return TryToEscape();
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
            yield return sourceUnit.Hud.WaitForHPUpdate();
            yield break;
        }
        yield return ShowStatusChanges(sourceUnit.Mon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Mon.Name} used {move.Base.Name}");
        
        //check if move hits
        if(CheckIfMoveHits(move, sourceUnit.Mon, targetUnit.Mon))
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            
            //play hit animation only if target is foe
            bool playHit = move.Base.Target == MoveTarget.Foe ? true : false;
            //same for secondary effect targets
            if(!playHit)
            {
                foreach(var effect in move.Base.SecondaryEffects)
                {
                    if(playHit = effect.Target == MoveTarget.Foe ? true : playHit)
                    {
                        break;
                    }
                }
            }
            //play hit animation or not
            if(playHit)
            {
                targetUnit.PlayHitAnimation();
            }

            if(move.Base.Category == MoveCategory.Status)
            {
                yield return RunMoveEffects(move.Base.Effects, sourceUnit.Mon, targetUnit.Mon, move.Base.Target);
            }
            else
            {
                var damageDetails = targetUnit.Mon.TakeDamage(move, sourceUnit.Mon);
                yield return targetUnit.Hud.WaitForHPUpdate();
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
                yield return HandleMonFainted(targetUnit);
            }
        }
        else
        {
            sourceUnit.PlayAttackAnimation();
            yield return new WaitForSeconds(1f);
            yield return dialogBox.TypeDialog($"{sourceUnit.Mon.Name}'s attack missed");
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
        yield return sourceUnit.Hud.WaitForHPUpdate();
        if(sourceUnit.Mon.HP <= 0)
        {
            yield return HandleMonFainted(sourceUnit);
            yield return new WaitUntil(() => state == BattleState.RunningTurn);
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

    IEnumerator HandleMonFainted(BattleUnit faintedUnit)
    {
        yield return dialogBox.TypeDialog($"{faintedUnit.Mon.Name} fainted");
        faintedUnit.PlayFaintAnimation();
        yield return new WaitForSeconds(2f);
        
        if(!faintedUnit.IsPlayerUnit)
        {
            //exp gain
            int expYield = faintedUnit.Mon.Base.ExpYield;
            int enemyLevel = faintedUnit.Mon.Level;
            float trainerBonus = (isTrainerBattle) ? 1.5f : 1f;

            int expGain = Mathf.FloorToInt((expYield * enemyLevel * trainerBonus) / 7);
            playerUnit.Mon.Exp += expGain;
            yield return dialogBox.TypeDialog($"{playerUnit.Mon.Name} gained {expGain} exp");
            yield return playerUnit.Hud.SetExpSmooth();
            
            //check level up
            while(playerUnit.Mon.CheckForLevelUp())
            {
                playerUnit.Hud.SetLevel();
                yield return dialogBox.TypeDialog($"{playerUnit.Mon.Name} grew to level {playerUnit.Mon.Level}");
                
                //try to learn a new move
                var newMoves = playerUnit.Mon.GetLearnableMovesAtCurrentLevel();
                if(newMoves.Count > 0)
                {
                    foreach(LearnableMove newMove in newMoves)
                    {
                            if(playerUnit.Mon.Moves.Count < MonBase.MaxNumberOfMoves)
                            {
                                playerUnit.Mon.LearnMove(newMove.Base);
                                yield return dialogBox.TypeDialog($"{playerUnit.Mon.Name} learned {newMove.Base.Name}");
                                dialogBox.SetMoveNames(playerUnit.Mon.Moves);
                                playerUnit.Mon.SetReadyForMove();
                            }
                            else
                            {
                                yield return dialogBox.TypeDialog($"{playerUnit.Mon.Name} is trying to learn {newMove.Base.Name}");
                                yield return dialogBox.TypeDialog($"But it can't learn more than {MonBase.MaxNumberOfMoves} moves");
                                yield return ChooseMoveToForget(playerUnit.Mon, newMove.Base);
                                yield return new WaitUntil(() => state != BattleState.MoveToForget);
                                yield return new WaitForSeconds(2f);
                            }
                        yield return new WaitUntil(() => playerUnit.Mon.ReadyForMove);
                    }
                }
                
                //check for evolution on each levelup
                var evolution = playerUnit.Mon.CheckForEvolution();
                if(evolution != null)
                {
                    void OnComplete()
                    {
                        playerUnit.Setup(playerUnit.Mon);
                        EvolutionManager.i.OnCompleteEvolution -= OnComplete;
                    }
                    Action onComplete = OnComplete;
                    EvolutionManager.i.OnCompleteEvolution += onComplete;
                    
                    yield return EvolutionManager.i.Evolve(playerUnit.Mon, evolution);
                }

                //refresh moves
                dialogBox.SetMoveNames(playerUnit.Mon.Moves);

                yield return playerUnit.Hud.SetExpSmooth(true);
            }

            //yield return new WaitForSeconds(1f);
        }

        CheckForBattleOver(faintedUnit);
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
                // player loses
                BattleOver(false);
            }
        }
        else
        {
            if(!isTrainerBattle)
            {
                BattleOver(true);
            }
            else
            {
                var nextMon = trainerParty.GetHealthyMon();
                if(nextMon != null)
                {
                    StartCoroutine(AboutToUse(nextMon));
                }
                else
                {
                    //player wins
                    
                    StartCoroutine(DefeatTrainer());

                    //Called this inside DefeatTrained coroutine instead
                    //BattleOver(true);
                }
            }
        }
    }

    IEnumerator DefeatTrainer()
    {
        trainerImage.gameObject.SetActive(true);
        trainerAnimatedImage = trainerImage.gameObject.GetComponent<AnimatedImage>();
        
        IEnumerator TrainerWantsToBattle()
        {
            animPlaying = true;
            Action finishAction = () => { animPlaying = false; };
            trainerAnimatedImage.transform.position = new Vector3(400, 0);
            yield return trainerAnimatedImage.ReturnToOriginalPosCoroutine(400, 0, 1.5f, finishAction);
        }
        state = BattleState.Busy;
        yield return TrainerWantsToBattle();
        yield return new WaitUntil(() => !animPlaying);
        
        //yield return DialogManager.Instance.ShowDialogText($"{player.Name} defeated {trainer.Name}!");
        yield return DialogManager.Instance.QueueDialogTextCoroutine($"{player.Name} defeated {trainer.Name}!");
        
        //! automatically include trainer name here in line below;
        //yield return DialogManager.Instance.ShowDialog(trainer.LoseDialog);
        yield return DialogManager.Instance.QueueDialogCoroutine(trainer.LoseDialog);
        
        if(trainer.BattleReward > 0)
        {
            player.Money += trainer.BattleReward;
            GameController.Instance.UpdateMoneyDisplay();
            //yield return DialogManager.Instance.ShowDialogText($"{player.Name} received ${trainer.BattleReward}!");
            yield return DialogManager.Instance.QueueDialogTextCoroutine($"{player.Name} received ${trainer.BattleReward}!");
        }

        BattleOver(true);
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
        else if(state == BattleState.Bag)
        {
            Action onBack = () =>
            {
                inventoryUI.gameObject.SetActive(false);
                state = BattleState.ActionSelection;
            };

            Action<ItemBase> onItemUsed = (ItemBase usedItem) =>
            {
                StartCoroutine(OnItemUsed(usedItem));
            };

            inventoryUI.HandleUpdate(onBack, onItemUsed);
        }
        else if(state == BattleState.AboutToUse)
        {
            HandleAboutToUse();
        }
        else if(state == BattleState.MoveToForget)
        {
            Action<int> onMoveSelected = (moveIndex) =>
            {
                moveSelectionUI.gameObject.SetActive(false);
                if(moveIndex == MonBase.MaxNumberOfMoves)
                {
                    //don't learn the new move
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Mon.Name} did not learn {moveToLearn.Name}"));
                    
                    //! wait for dialogBox.TypeDialog to conclude?
                    //playerUnit.Mon.SetReadyForMove();
                }
                else
                {
                    //forget the selected move and learn new move
                    var selectedMove = playerUnit.Mon.Moves[moveIndex].Base;
                    StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Mon.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}"));
                    
                    playerUnit.Mon.Moves[moveIndex] = new Move(moveToLearn);
                    
                    //! wait for dialogBox.TypeDialog to conclude?
                    //playerUnit.Mon.SetReadyForMove();
                }
                //! wait for dialogBox.TypeDialog to conclude?
                playerUnit.Mon.SetReadyForMove();
                
                moveToLearn = null;
                state = BattleState.RunningTurn;
            };

            moveSelectionUI.HandleMoveSelection(onMoveSelected);
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
                OpenBag();
            }
            else if(currentAction == 2)
            {
                //Mon
                OpenPartyScreen();
            }
            else if(currentAction == 3)
            {
                //Run
                StartCoroutine(RunTurns(BattleAction.Run));
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
        Action onSelected = () =>
        {
            var selectedMember = partyScreen.SelectedMember;
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

            if(partyScreen.CalledFrom == BattleState.ActionSelection)
            {
                StartCoroutine(RunTurns(BattleAction.SwitchMon));
            }
            else
            {
                state = BattleState.Busy;
                bool isTrainerAboutToUse = partyScreen.CalledFrom == BattleState.AboutToUse;
                StartCoroutine(SwitchMon(selectedMember, isTrainerAboutToUse));
            }

            partyScreen.CalledFrom = null;
        };

        Action onBack = () =>
        {
            if(playerUnit.Mon.HP <= 0)
            {
                partyScreen.SetMessageText("You have to choose mon to continue");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            if(partyScreen.CalledFrom == BattleState.AboutToUse)
            {
                StartCoroutine(SendNextTrainerMon());
            }
            else
            {
                ActionSelection();
            }

            partyScreen.CalledFrom = null;
        };

        partyScreen.HandleUpdate(onSelected, onBack);
    }

    private void HandleAboutToUse()
    {
        if(Input.GetButtonDown("Up") || Input.GetButtonDown("Down"))
        {
            aboutToUseChoice = !aboutToUseChoice;
        }
        dialogBox.UpdateChoiceBox(aboutToUseChoice);

        if(Input.GetButtonDown("Submit"))
        {
            dialogBox.EnableChoiceBox(false);
            if(aboutToUseChoice == true)
            {
                //yes option
                OpenPartyScreen();
            }
            else
            {
                //no option
                StartCoroutine(SendNextTrainerMon());
            }
        }
        else if(Input.GetButtonDown("Cancel"))
        {
            dialogBox.EnableChoiceBox(false);
            StartCoroutine(SendNextTrainerMon());
        }
    }

    IEnumerator SwitchMon(Mon newMon, bool isTrainerAboutToUse = false)
    {
        if(playerUnit.Mon.HP > 0)
        {
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Mon.Name}");
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.Setup(newMon);
        dialogBox.SetMoveNames(newMon.Moves);
        yield return dialogBox.TypeDialog($"Go {newMon.Name}!");

        if(isTrainerAboutToUse)
        {
            StartCoroutine(SendNextTrainerMon());
        }
        else
        {
            state = BattleState.RunningTurn;
        }
    }

    IEnumerator SendNextTrainerMon()
    {
        state = BattleState.Busy;

        var nextMon = trainerParty.GetHealthyMon();
        enemyUnit.Setup(nextMon);
        yield return dialogBox.TypeDialog($"{trainer.Name} sent out {nextMon.Name}");

        state = BattleState.RunningTurn;
    }

    IEnumerator OnItemUsed(ItemBase usedItem)
    {
        state = BattleState.Busy;
        inventoryUI.gameObject.SetActive(false);

        if(usedItem is CapsuleItem)
        {
            yield return ThrowCapsule((CapsuleItem)usedItem);
        }

        StartCoroutine(RunTurns(BattleAction.UseItem));
    }

    IEnumerator ThrowCapsule(CapsuleItem capsuleItem)
    {
        state = BattleState.Busy;

        if(isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't steal another trainer's mons!");
            state = BattleState.RunningTurn;
            yield break;
        }

        yield return dialogBox.TypeDialog($"{player.Name} tossed a {capsuleItem.Name.ToUpper()}!");

        var capsuleObj = Instantiate(capsuleSprite, playerUnit.transform.position - new Vector3(2, 0), Quaternion.identity);
        var capsule = capsuleObj.GetComponent<SpriteRenderer>();
        capsule.sprite = capsuleItem.Icon;

        // Animations
        yield return capsule.transform.DOJump(enemyUnit.transform.position + new Vector3(0, 2), 2f, 1, 1f).WaitForCompletion();
        yield return enemyUnit.PlayCaptureAnimation();
        yield return capsule.transform.DOMoveY(enemyUnit.transform.position.y - 1.3f, 0.5f).WaitForCompletion();

        int shakeCount = TryToCatchMon(enemyUnit.Mon, capsuleItem);

        for(int i = 0; i < Mathf.Min(shakeCount, 3); ++i)
        {
            yield return new WaitForSeconds(0.5f);
            yield return capsule.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.8f).WaitForCompletion();
        }

        if(shakeCount == 4)
        {
            // Mon was caught
            yield return dialogBox.TypeDialog($"{enemyUnit.Mon.Name} was caught!");
            
            yield return capsule.DOFade(0, 1.5f).WaitForCompletion();
            //SpriteFader.FadeSprite(capsule, 0.01f);
            //yield return new WaitForSeconds(0.5f);

            playerParty.AddMon(enemyUnit.Mon);
            yield return dialogBox.TypeDialog($"{enemyUnit.Mon.Name} has been added to your party");

            //!
            GameController.Instance.OpenNicknameMenu(enemyUnit.Mon, GameState.FreeRoam);
            yield return new WaitUntil(()=>GameController.Instance.state == GameState.ConfirmationMenu);
            yield return new WaitUntil(()=>GameController.Instance.state != GameState.ConfirmationMenu);
            
            if(GameController.Instance.state == GameState.SetMonNick)
            {
                yield return new WaitUntil(()=>GameController.Instance.state != GameState.SetMonNick);
            }

            Destroy(capsule);
            BattleOver(true);
        }
        else
        {
            // Mon broke out
            yield return new WaitForSeconds(1f);
            
            capsule.DOFade(0, 0.2f);
            //SpriteFader.FadeSprite(capsule, 0.01f);
            
            yield return enemyUnit.PlayBreakoutAnimation();
            
            if(shakeCount < 2)
            {
                yield return dialogBox.TypeDialog($"{enemyUnit.Mon.Name} broke free!");
            }
            else
            {
                yield return dialogBox.TypeDialog($"Almost caught it!");
            }

            Destroy(capsule);
            state = BattleState.RunningTurn;
        }
    }

    private int TryToCatchMon(Mon mon, CapsuleItem capsuleItem)
    {
        float a = (3 * mon.MaxHp - 2 * mon.HP) * mon.Base.CatchRate * capsuleItem.CatchRateModifier * ConditionsDB.GetStatusBonus(mon.Status) / (3 * mon.MaxHp);

        if(a >= 255)
        {
            return  4;
        }
        
        float b = 1048560 / Mathf.Sqrt(Mathf.Sqrt(16711680 / a));

        int shakeCount = 0;
        while(shakeCount < 4)
        {
            if(UnityEngine.Random.Range(0, 65535) >= b)
            {
                break;
            }
            else
            {
                ++shakeCount;
            }
        }

        return shakeCount;
    }

    IEnumerator TryToEscape()
    {
        state = BattleState.Busy;
        if(isTrainerBattle)
        {
            yield return dialogBox.TypeDialog($"You can't run from trainer battles!");
            state = BattleState.RunningTurn;
            yield break;
        }

        ++escapeAttempts;

        int playerSpeed = playerUnit.Mon.Speed;
        int enemySpeed = enemyUnit.Mon.Speed;

        if(enemySpeed < playerSpeed)
        {
            yield return dialogBox.TypeDialog($"Ran away safely!");
            BattleOver(true);
        }
        else
        {
            float f = (playerSpeed * 128) / enemySpeed + 30 * escapeAttempts;
            f = f % 256;

            if(UnityEngine.Random.Range(0, 256) < f)
            {
                yield return dialogBox.TypeDialog($"Ran away safely!");
                BattleOver(true);
            }
            else
            {
                yield return dialogBox.TypeDialog($"Can't escape!");
                state = BattleState.RunningTurn;
            }
        }
    }
}
