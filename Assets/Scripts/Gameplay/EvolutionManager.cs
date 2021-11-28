using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EvolutionManager : MonoBehaviour
{
    [SerializeField] GameObject evolutionUI;
    [SerializeField] Image monImage;

    public event Action OnStartEvolution;
    public event Action OnCompleteEvolution;

    public static EvolutionManager i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public IEnumerator Evolve(Mon mon, Evolution evolution)
    {
        OnStartEvolution?.Invoke();

        evolutionUI.SetActive(true);
        monImage.sprite = mon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{mon.Name} is evolving!");

        var oldMonName = mon.Name;
        mon.Evolve(evolution);

        monImage.sprite = mon.Base.FrontSprite;
        yield return DialogManager.Instance.ShowDialogText($"{oldMonName} evolved into {mon.Base.Name}");

        evolutionUI.SetActive(false);

        //I Added this to fix name not updating in partylist after evolving with evolutionItem
        MonParty.GetPlayerParty().UpdateParty();

        OnCompleteEvolution?.Invoke();
    }
}
