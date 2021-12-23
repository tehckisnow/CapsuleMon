using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    
    [SerializeField] ItemBase bikeScriptableObject;

    [SerializeField] int money;
    public int Money {
        get { return money; }
        set { money = value; }
    }

    [SerializeField] int stepsPerPoisonDamage = 4;
    private int stepsForPoison;

    private bool isMoving;
    private bool isRunning;
    private Vector2 input;

    private Animator animator;
    private Character character;

    private bool onBike = false;
    public bool OnBike => onBike;

    private bool isOutside = true;
    public bool IsOutside {
        get { return isOutside; }
        set { isOutside = value; }
    }

    private MonParty playerParty;

    [SerializeField] private bool cheatsOn = false;

    public static PlayerController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        playerParty = MonParty.GetPlayerParty();
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            if(cheatsOn)
            {
                GameController.Instance.OpenCheatMenu();
            }
        }
        if(Input.GetKeyDown(KeyCode.BackQuote))
        {
            OpenConsole();
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            ToggleBike();
        }

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

        animator.SetBool("onBike", onBike);
        character.IsBiking = onBike;

        character.HandleUpdate();

        if(Input.GetButtonDown("Submit"))
        {
            StartCoroutine(Interact());
        }
    }

    IEnumerator DrawCicle(Vector3 interactPos)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.position = interactPos;
        cylinder.transform.rotation = Quaternion.Euler(90, 0, 0);
        cylinder.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);

        yield return new WaitForSeconds(2f);

        Destroy(cylinder);
    }

    private void OpenConsole()
    {
        GameController.Instance.OpenConsole();
    }

    private IEnumerator Interact()
    {
        
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        //prevent interactPos from floating up into next tile above
        if(character.Facing == Facing.Up)
        {
            facingDir.y -= 0.4f;
        }
        var interactPos = transform.position + facingDir;
        
        //Debug
        // StartCoroutine(DrawCicle(transform.position));
        // StartCoroutine(DrawCicle(interactPos));

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

        CheckForPoison();
    }

    private void CheckForPoison()
    {
        if(playerParty != null && playerParty.Mons.Count > 0)
        {
            stepsForPoison++;
            if(stepsForPoison >= stepsPerPoisonDamage)
            {
                stepsForPoison = 0;
                StartCoroutine(CheckMonsForPoison());
            }
        }
    }

    private IEnumerator CheckMonsForPoison()
    {
        foreach(Mon mon in playerParty.Mons)
        {
            if(mon.Status != null && mon.Status.Id == ConditionID.psn)
            {
                //play poison sound effect
                //flash screen
                Fader.Instance.SetColor(Fader.Instance.Color4);
                yield return Fader.Instance.FadeIn(0.2f);
                Fader.Instance.SetColor(Fader.Instance.DefaultColor);
                mon.UpdateHP(1);
                //check if fainted
                if(mon.HP < 1 && !mon.isFainted)
                {
                    mon.isFainted = true;
                    yield return DialogManager.Instance.QueueDialogTextCoroutine($"{mon.Name} has fainted!");
                    //check for other useable mon
                    var nextMon = playerParty.GetHealthyMon();
                    if(nextMon == null)
                    {
                        //whiteout and tp to last waypoint
                        var warpController = GetComponent<WarpController>();
                        yield return warpController.GoToLastWarpAnim();
                        break;
                    }
                }
            }
        }
    }

    public bool SpendMoney(int amount)
    {
        if(amount <= money)
        {
            money -= amount;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void GetOnBike()
    {
        if(!onBike)// && CheckForBike())
        {
            if(IsOutside)
            {
                onBike = true;
            }
            else
            {
                StartCoroutine(IndoorBikeMessage());
            }
        }
    }

    IEnumerator IndoorBikeMessage()
    {
        //yield return DialogManager.Instance.ShowDialogText($"Prof: HEY! No biking inside!");
        yield return DialogManager.Instance.QueueDialogTextCoroutine($"Prof: HEY! No biking inside!");
        //yield return DialogManager.Instance.ShowDialogText($"Woah!");
        yield return DialogManager.Instance.QueueDialogTextCoroutine($"Woah!");
        //yield return DialogManager.Instance.ShowDialogText($"Where did he come from?");
        yield return DialogManager.Instance.QueueDialogTextCoroutine($"Where did he come from?");
    }

    public void GetOffBike()
    {
        onBike = false;
    }

    public void ToggleBike()
    {
        if(CheckForBike())
        {
            if(onBike)
            {
                GetOffBike();
            }
            else
            {
                GetOnBike();
            }
        }
    }

    public bool CheckForBike()
    {
        var inventory = GetComponent<Inventory>();
        if(inventory)
        {
            return inventory.HasItem(bikeScriptableObject);
        }
        else return false;
    }

    public void CheatsOn()
    {
        cheatsOn = true;
    }
    public void CheatsOff()
    {
        cheatsOn = false;
    }

    public object CaptureState()
    {
        var facing = FacingClass.GetXY(character.Facing);

        Vector2 warpPoint = GetComponent<WarpController>().GetWarpPoint();

        var saveData = new PlayerSaveData()
        {
            name = name,
            money = money,
            position = new float[] {transform.position.x, transform.position.y, facing.x, facing.y},
            mons = GetComponent<MonParty>().Mons.Select(p => p.GetSaveData()).ToList(),
            storage = GetComponent<MonStorage>().Mons.Select(p => p.GetSaveData()).ToList(),
            onBike = onBike,
            warpPointx = warpPoint.x,
            warpPointy = warpPoint.y
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

        var storage = GetComponent<MonStorage>();
        if(storage != null)
        {
            storage.Mons = saveData.storage.Select(s => new Mon(s)).ToList();
        }

        onBike = saveData.onBike;

        var warpController = GetComponent<WarpController>();
        if(warpController != null)
        {
            warpController.SetWarpPoint(new Vector2(saveData.warpPointx, saveData.warpPointy));
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
    public List<MonSaveData> storage;
    public bool onBike;
    public float warpPointx;
    public float warpPointy;
}