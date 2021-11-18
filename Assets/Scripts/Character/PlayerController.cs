using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float runModifier = 2;
    
    public event Action OnEncountered;

    private bool isMoving;
    private bool isRunning;
    private Vector2 input;

    private Animator animator;
    //private CharacterAnimator animator2;
    private Character character;

    private void Start()
    {
        animator = GetComponent<Animator>();
        //animator2 = GetComponent<CharacterAnimator>();
        character = GetComponent<Character>();
    }

    public void HandleUpdate()
    {
        //if(!isMoving)
        if(!character.IsMoving)
        {
            //running
            if(Input.GetButton("Fire3"))
            {
                animator.SetBool("isRunning", true);
                //isRunning = true;
                character.IsRunning = true;
            }
            else
            {
                animator.SetBool("isRunning", false);
                //isRunning = false;
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
                StartCoroutine(character.Move(input, CheckForEncounters));

                // animator.SetFloat("moveX", input.x);
                // animator.SetFloat("moveY", input.y);

                // var targetPos = transform.position;
                // targetPos.x += input.x;
                // targetPos.y += input.y;

                // //use testPos to apply vertical offset to test closer to ground
                // float verticalOffset = -0.7f;
                // var testPos = new Vector3(targetPos.x, targetPos.y + verticalOffset, 0f);
                // if(IsWalkable(testPos))
                // {
                //     StartCoroutine(Move(targetPos));
                // }


            }
        }
        //animator.SetBool("isMoving", isMoving);

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
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

    // IEnumerator Move(Vector3 targetPos)
    // {
    //     isMoving = true;

    //     float run = 1f;
    //     if(isRunning)
    //     {
    //         run = runModifier;
    //     }
    //     while((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
    //     {
    //         transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * run * Time.deltaTime);
    //         yield return null;
    //     }
    //     transform.position = targetPos;

    //     isMoving = false;

    //     CheckForEncounters();
    // }

    // private bool IsWalkable(Vector3 targetPos)
    // {
    //     if(Physics2D.OverlapCircle(targetPos, 0.2f, collisionsLayer | interactablesLayer) != null)
    //     {
    //         return false;
    //     }
    //     if(Physics2D.OverlapCircle(targetPos, 0.2f, walkableLayer) == null)
    //     {
    //         return false;
    //     }
    //     return true;
    // }

    private void CheckForEncounters()
    {
        if(Physics2D.OverlapCircle(transform.position, 0.2f, GameLayers.Instance.GrassLayer) != null)
        {
            if(UnityEngine.Random.Range(1, 101) <= 10)
            {
                animator.SetBool("isMoving", false);
                OnEncountered();
            }
        }
    }
}
