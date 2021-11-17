using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float runModifier = 2;
    [SerializeField] LayerMask collisionsLayer;
    [SerializeField] LayerMask grassLayer;
    
    public event Action OnEncountered;

    private bool isMoving;
    private bool isRunning;
    private Vector2 input;

    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        
        //just a remind about the .8 y offset
        Vector3 pos = new Vector3(0.5f, 0.8f, 0f);
        transform.position = pos;
    }

    public void HandleUpdate()
    {
        if(!isMoving)
        {
            //running
            if(Input.GetButton("Fire3"))
            {
                animator.SetBool("isRunning", true);
                isRunning = true;
            }
            else
            {
                animator.SetBool("isRunning", false);
                isRunning = false;
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
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if(IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
            }
        }
        animator.SetBool("isMoving", isMoving);
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        float run = 1f;
        if(isRunning)
        {
            run = runModifier;
        }
        while((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * run * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;

        CheckForEncounters();
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if(Physics2D.OverlapCircle(targetPos, 0.2f, collisionsLayer) != null)
        {
            return false;
        }
        return true;
    }

    private void CheckForEncounters()
    {
        if(Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
            if(UnityEngine.Random.Range(1, 101) <= 10)
            {
                animator.SetBool("isMoving", false);
                OnEncountered();
            }
        }
    }
}
