using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float runModifier = 2;

    private bool isMoving;
    private bool isRunning;
    private Vector2 input;

    private Animator animator;
    private Character character;

    private void Start()
    {
        animator = GetComponent<Animator>();
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if(!character.IsMoving)
        {
            //running
            if(Input.GetButton("Fire3"))
            {
                animator.SetBool("isRunning", true);
                character.IsRunning = true;
            }
            else
            {
                animator.SetBool("isRunning", false);
                character.IsRunning = false;
            }

            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //remove diagonal movement
            if(input.x != 0)
            {
                input.y = 0;
            }

            if(input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if(Input.GetButtonDown("Submit"))
        {
            Interact();
        }
    }

    private void Interact()
    {
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.red, 0.5f);
        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.Instance.InteractablesLayer);
        if(collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.Instance.TriggerableLayers);
        
        foreach(var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if(triggerable != null)
            {
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
    }

    public object CaptureState()
    {
        var facing = FacingClass.GetXY(character.Facing);

        var saveData = new PlayerSaveData()
        {
            position = new float[] {transform.position.x, transform.position.y, facing.x, facing.y},
            mons = GetComponent<MonParty>().Mons.Select(p => p.GetSaveData()).ToList()
        };

        Debug.Log("exp: " + saveData.mons[0].exp + " HP: " + saveData.mons[0].hp);
        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;
        Debug.Log("exp: " + saveData.mons[0].exp + " HP: " + saveData.mons[0].hp);

        // Restore position
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);
        Facing facing = FacingClass.GetFacing(pos[2], pos[3]);
        //Debug.Log("setting to: " + facing.ToString());
        character.Facing = facing;
        //Debug.Log(character.Facing.ToString()); //!WTF?  is something else overriding this state later?

        // Restore party
        GetComponent<MonParty>().Mons = saveData.mons.Select(s => new Mon(s)).ToList();
    }

    public string Name {
        get => name;
    }

    public Sprite Sprite {
        get => sprite;
    }

    public Character Character => character;
}

[Serializable]
public class PlayerSaveData
{
    public float[] position;
    public List<MonSaveData> mons;
}