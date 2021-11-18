using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float runModifier = 2f;

    public bool IsMoving { get; private set; }
    public bool IsRunning {get; set; }

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null)
    {
        animator.SetFloat("moveX", Mathf.Clamp(moveVec.x, -1, 1));
        animator.SetFloat("moveY", Mathf.Clamp(moveVec.y, -1, 1));

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if(!IsWalkable(targetPos))
        {
            yield break;
        }

        IsMoving = true;

        float run = 1f;
        if(IsRunning)
        {
            run = runModifier;
        }
        while((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * run * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        IsMoving = false;

        //CheckForEncounters();
        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.SetBool("isMoving", IsMoving);
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if(Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.Instance.CollisionsLayer | GameLayers.Instance.InteractablesLayer) != null)
        {
            return false;
        }
        if(Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.Instance.WalkableLayer) == null)
        {
            return false;
        }
        return true;
    }
}
