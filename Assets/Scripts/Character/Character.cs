using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float runModifier = 2f;
    [SerializeField] Facing facing = Facing.Down;
    public Facing Facing {
        get { return facing; }
        set {}
    }
    [SerializeField] Transform originTransform;

    public float OffsetY {get; private set; } = 0.3f;

    public bool IsMoving { get; set; }
    public bool IsRunning {get; set; }

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        SetPositionAndSnapToTile(transform.position);
    }

    private void Update()
    {
        var dir = FacingClass.GetXY(facing);
        animator.SetFloat("moveX", dir.x);
        animator.SetFloat("moveY", dir.y);
    }

    public void SetPositionAndSnapToTile(Vector2 pos)
    {
        pos.x = Mathf.Floor(pos.x) + 0.5f;
        pos.y = Mathf.Floor(pos.y) + 0.5f + OffsetY;

        transform.position = pos;
    }
    
    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver = null)
    {
        var clampedX = Mathf.Clamp(moveVec.x, -1, 1);
        var clampedY = Mathf.Clamp(moveVec.y, -1, 1);
        animator.SetFloat("moveX", clampedX);
        animator.SetFloat("moveY", clampedY);
        facing = FacingClass.GetFacing(clampedX, clampedY);

        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        if(!IsPathClear(targetPos))
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

    //these are for drawing a gizmo to visualize the IsPathClear box
    private Vector2 gizmoDir;
    private Vector2 gizmoBox;

    private bool IsPathClear(Vector3 targetPos)
    {
        //draw a box along the entire path and check for collisions in it
        var diff = targetPos - transform.position;
        var dir = diff.normalized;
        
        var origin = new Vector2(0,0);
        if(originTransform == null)
        {
            origin = transform.position + dir;
        }
        else
        {
            origin.x = originTransform.position.x + dir.x;
            origin.y = originTransform.position.y + dir.y;
        }

        var boxSize = new Vector2(.2f, .2f);
        gizmoBox = boxSize;
        gizmoDir = dir;

        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.NoFilter();
        contactFilter.layerMask = GameLayers.Instance.CollisionsLayer | GameLayers.Instance.InteractablesLayer | GameLayers.Instance.PlayerLayer;
        contactFilter.useLayerMask = true;

        List<RaycastHit2D> results = new List<RaycastHit2D>();
        int hit = Physics2D.BoxCast
        (
            //transform.position + dir,
            origin,
            boxSize,
            0f,
            dir,
            contactFilter,
            results,
            diff.magnitude - 1
        );

        if(hit > 0)
        {
            foreach(RaycastHit2D result in results)
            {
                if(result.collider.gameObject != this.gameObject)
                {
                    return false;
                }
            }
        }
        return true;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position + new Vector3(gizmoDir.x, gizmoDir.y), new Vector3(gizmoBox.x, gizmoBox.y, 1f));
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if(Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.Instance.InteractablesLayer | GameLayers.Instance.CollisionsLayer) != null)
        {
            return false;
        }
        if(Physics2D.OverlapCircle(targetPos, 0.2f, GameLayers.Instance.WalkableLayer) == null)
        {
            return false;
        }
        return true;
    }

    public void LookTowards(Vector3 targetPos)
    {
        var xdiff = Mathf.Floor(targetPos.x) - Mathf.Floor(transform.position.x);
        var ydiff = Mathf.Floor(targetPos.y) - Mathf.Floor(transform.position.y);
        if(xdiff == 0 || ydiff == 0)
        {
            var clampedX = Mathf.Clamp(xdiff, -1f, 1f);
            var clampedY = Mathf.Clamp(ydiff, -1f, 1f);
            animator.SetFloat("moveX", clampedX);
            animator.SetFloat("moveY", clampedY);
            facing = FacingClass.GetFacing(clampedX, clampedY);
        }
        else
        {
            Debug.LogError("Error in Look Towards: You cannot ask a character to look diagonally");
        }
    }
}

//this is duplicated in CharacterAnimator as FacingDirection
public enum Facing { Up, Down, Left, Right }

public class FacingClass
{
    public static Facing GetFacing(Vector2 dir)
    {
        return GetFacing(dir.x, dir.y);
    }

    public static Facing GetFacing(float x, float y)
    {
        return GetFacing((int)x, (int)y);
    }

    public static Facing GetFacing(int x, int y)
    {
        if(x == -1 && y == 0)
        {
            return Facing.Left;
        }
        if(x == 1 && y == 0)
        {
            return Facing.Right;
        }
        if(x == 0 && y == 1)
        {
            return Facing.Up;
        }
        if(x == 0 && y == -1)
        {
            return Facing.Down;
        }
            return Facing.Down;
    }

    public static Vector2 GetXY(Facing facing)
    {
        switch(facing)
        {
            case Facing.Left:
                return new Vector2(-1, 0);
            case Facing.Right:
                return new Vector2(1, 0);
            case Facing.Up:
                return new Vector2(0, 1);
            case Facing.Down:
            default:
                return new Vector2(0, -1);
        }
    }
}
