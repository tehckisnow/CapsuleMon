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
        
        //yield return DialogManager.Instance.ShowDialogText($"{mon.Name} is evolving!");
        yield return DialogManager.Instance.QueueDialogTextCoroutine($"{mon.Name} is evolving!");
        isEvolving = true;

        //mon.Evolve(evolution);
        
        evolutionCoroutine = StartCoroutine(animImage.TransitionCoroutine(newMonBase.FrontSprite, 5f));
        yield return new WaitUntil(() => animImage.inTransition != true);

        //monImage.sprite = mon.Base.FrontSprite;

        isEvolving = false;

        if(evolutionSuccess)
        {
            mon.Evolve(evolution);
            //yield return DialogManager.Instance.ShowDialogText($"{oldMonName} evolved into {mon.Base.Name}!");
            yield return DialogManager.Instance.QueueDialogTextCoroutine($"{oldMonName} evolved into {mon.Base.Name}!");
            if(rename)
            {
                mon.Name = mon.Base.Name;
            }
            yield return mon.CheckForEvolutionMove();
        }
        else
        {
            monImage.sprite = mon.Base.FrontSprite;
            //yield return DialogManager.Instance.ShowDialogText($"{oldMonName} stopped evolving!");
            yield return DialogManager.Instance.QueueDialogTextCoroutine($"{oldMonName} stopped evolving!");
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

}
