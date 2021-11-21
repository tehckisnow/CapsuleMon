﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fovPivot;
    
    // State
    private bool battleLost = false;

    Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Facing);
    }

    public void Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);
        if(!battleLost)
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => 
            {
                GameController.Instance.StartTrainerBattle(this);
            }));
        }
        else
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialogAfterBattle));
        }
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        //show exclamation
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        //walk towards player
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        //show dialog
        StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => {
            GameController.Instance.StartTrainerBattle(this);
        }));
    }

    public void BattleLost()
    {
        battleLost = true;
        fovPivot.gameObject.SetActive(false);
    }

    public void SetFovRotation(Facing dir)
    {
        float angle = 0f;
        if(dir == Facing.Right)
            angle = 90f;
        else if(dir == Facing.Right)
            angle = 180f;
        else if(dir == Facing.Left)
            angle = 270f;
    
        fovPivot.transform.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    public string Name {
        get => name;
    }

    public Sprite Sprite {
        get => sprite;
    }
}
