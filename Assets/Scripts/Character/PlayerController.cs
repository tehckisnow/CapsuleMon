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

    [SerializeField] int money;
    public int Money {
        get { return money; }
        set { money = value; }
    }

    private bool isMoving;
    private bool isRunning;
    private Vector2 input;

    private Animator animator;
    private Character character;

    public static PlayerController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

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
            StartCoroutine(Interact());
        }
    }

    private IEnumerator Interact()
    {
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.red, 0.5f);
        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.Instance.InteractablesLayer);
        if(collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    IPlayerTriggerable currentlyInTrigger;

    private void OnMoveOver()
    {
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.Instance.TriggerableLayers);
        
        IPlayerTriggerable triggerable = null;
        foreach(var collider in colliders)
        {
            triggerable = collider.GetComponent<IPlayerTriggerable>();
            if(triggerable != null)
            {
                if(triggerable == currentlyInTrigger && !triggerable.TriggerRepeatedly)
                {
                    break;
                }

                triggerable.OnPlayerTriggered(this);
                currentlyInTrigger = triggerable;
                break;
            }
        }
        //clear currentlyInTrigger if player leaves trigger
        if(colliders.Count() == 0 || triggerable != currentlyInTrigger)
        {
            currentlyInTrigger = null;
        }
    }

    public object CaptureState()
    {
        var facing = FacingClass.GetXY(character.Facing);

        var saveData = new PlayerSaveData()
        {
            name = name,
            money = money,
            position = new float[] {transform.position.x, transform.position.y, facing.x, facing.y},
            mons = GetComponent<MonParty>().Mons.Select(p => p.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;
        
        name = saveData.name;
        money = saveData.money;

        // Restore position
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);
        if(character != null)
        {
            Facing facing = FacingClass.GetFacing(pos[2], pos[3]);
            character.Facing = facing;
        }

        var gameController = GameController.Instance;
        if(gameController != null)
        {
            GameController.Instance.UpdateMoneyDisplay();
            GameController.Instance.UpdateNameDisplay();
        }

        // Restore party
        var monParty = GetComponent<MonParty>();
        if(monParty != null)
        {
            monParty.Mons = saveData.mons.Select(s => new Mon(s)).ToList();
        }
    }

    public string Name {
        get => name;
        set => name = value;
    }

    public Sprite Sprite {
        get => sprite;
    }

    public Character Character => character;
}

[Serializable]
public class PlayerSaveData
{
    public string name;
    public int money;
    public float[] position;
    public List<MonSaveData> mons;
}