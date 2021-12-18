using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image monImage;
    
    private AnimatedImage animImage;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;

    public static EvolutionManager i { get; private set; }

    private bool isEvolving = false;
    private Coroutine evolutionCoroutine;

    private bool evolutionSuccess;
    public bool EvolutionSuccess => evolutionSuccess;

    private void Awake()
    {
        i = this;
    }

    private void Update()
    {
        HandleUpdate();
    }

    public void HandleUpdate()
    {
        if(Input.GetButtonDown("Cancel"))
        {
            SkipEvolution();
        }
    }

    public IEnumerator Evolve(Mon mon, Evolution evolution)
    {
        evolutionSuccess = true;
        OnStartEvolution?.Invoke();

        evolutionUI.SetActive(true);
        monImage.sprite = mon.Base.FrontSprite;
        
        bool rename = mon.Name == mon.Base.Name;
        var oldMonName = mon.Name;
        var newMonBase = evolution.EvolveInto;
        animImage = monImage.GetComponent<AnimatedImage>();
        
        yield return DialogManager.Instance.ShowDialogText($"{mon.Name} is evolving!");
        isEvolving = true;

        //mon.Evolve(evolution);
        
        evolutionCoroutine = StartCoroutine(animImage.TransitionCoroutine(newMonBase.FrontSprite, 5f));
        yield return new WaitUntil(() => animImage.inTransition != true);

        //monImage.sprite = mon.Base.FrontSprite;

        isEvolving = false;

        if(evolutionSuccess)
        {
            mon.Evolve(evolution);
            yield return DialogManager.Instance.ShowDialogText($"{oldMonName} evolved into {mon.Base.Name}!");
            if(rename)
            {
                mon.Name = mon.Base.Name;
            }
            yield return mon.CheckForEvolutionMove();
        }
        else
        {
            monImage.sprite = mon.Base.FrontSprite;
            yield return DialogManager.Instance.ShowDialogText($"{oldMonName} stopped evolving!");
        }
        
        //yield return DialogManager.Instance.ShowDialogText($"{oldMonName} evolved into {mon.Base.Name}");
        
        //evolutionUI.SetActive(false);
        CloseEvolutionUI();

        //I Added this to fix name not updating in partylist after evolving with evolutionItem
        MonParty.GetPlayerParty().UpdateParty();

        OnCompleteEvolution?.Invoke();
    }

    public void SkipEvolution()
    {
        if(isEvolving)
        {
            isEvolving = false;
            StopCoroutine(evolutionCoroutine);
            if(animImage != null)
            {
                animImage.inTransition = false;
            }
            //prevent evolution
            evolutionSuccess = false;
        }
    }

    public void CloseEvolutionUI()
    {
        evolutionCoroutine = null;
        evolutionUI.SetActive(false);
    }

    // IEnumerator CheckForEvolutionMove(Mon mon)
    // {
    //     var moves = mon.Base.MovesLearnedUponEvolution;
    //     foreach(MoveBase move in moves)
    //     {
    //         if(move != null)
    //         {
    //             if(mon.Moves.Count < MonBase.MaxNumberOfMoves)
    //             {
    //                 mon.LearnMove(move);
    //                 yield return DialogManager.Instance.ShowDialogText($"{mon.Name} learned {move.Name}");
    //             }
    //             else
    //             {
    //                 yield return DialogManager.Instance.ShowDialogText($"{mon.Name} is trying to learn {move.Name}");
    //                 yield return DialogManager.Instance.ShowDialogText($"But it can't learn more than {MonBase.MaxNumberOfMoves} moves");
    //                 //yield return ChooseMoveToForget(mon, move);
    //                 //yield return new WaitUntil(() => State != BattleState.MoveToForget);
    //                 //yield return new WaitForSeconds(2f);
    //             }
    //         }
    //     }

        // IEnumerator ChooseMoveToForget(Mon mon, MoveBase newMove)
        // {
        //     state = BattleState.Busy;
        //     yield return DialogManager.Instance.ShowDialogText($"Choose a move you want to forget");
        //     moveSelectionUI.gameObject.SetActive(true);
        //     moveSelectionUI.SetMoveData(mon.Moves.Select(x => x.Base).ToList(), newMove);
        //     moveToLearn = newMove;

        //     state = BattleState.MoveToForget;
        // }

        // void CheckState()
        // {
        //     if(state == BattleState.MoveToForget)
        //     {
        //         Action<int> onMoveSelected = (moveIndex) =>
        //         {
        //             moveSelectionUI.gameObject.SetActive(false);
        //             if(moveIndex == MonBase.MaxNumberOfMoves)
        //             {
        //                 //don't learn the new move
        //                 StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Mon.Name} did not learn {moveToLearn.Name}"));
        //             }
        //             else
        //             {
        //                 //forget the selected move and learn new move
        //                 var selectedMove = playerUnit.Mon.Moves[moveIndex].Base;
        //                 StartCoroutine(dialogBox.TypeDialog($"{playerUnit.Mon.Name} forgot {selectedMove.Name} and learned {moveToLearn.Name}"));
                        
        //                 playerUnit.Mon.Moves[moveIndex] = new Move(moveToLearn);
        //             }
        //             moveToLearn = null;
        //             state = BattleState.RunningTurn;
        //         };

        //         moveSelectionUI.HandleMoveSelection(onMoveSelected);
        //     }
        // }

    //}
}
