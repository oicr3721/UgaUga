using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Character : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Animator animator;

    [Header("Movement")]
    [SerializeField] protected float moveSpeed = 1f;

    protected Vector2 facingDirection = Vector2.right;
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Animator Animator => animator;

    private Vector2 moveInput;
    private bool canMove = true;

    protected void Update()
    {
        Tick();

        if (!canMove || moveInput == Vector2.zero)
            return;

        Vector3 move =
            (Vector3)moveInput *
            moveSpeed *
            Time.deltaTime;

        transform.position += move;
    }

    Vector2 prevInput;
    public void SetMoveInput(Vector2 input)
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = input;

        animator.SetBool(
            "Moving",
            input != Vector2.zero
        );

        Flip(input);
    }

    public void Flip(Vector2 dir)
    {
        if (dir.x == 0f || spriteRenderer == null)
            return;

        facingDirection =
            dir.x < 0f
            ? Vector2.left
            : Vector2.right;

        spriteRenderer.flipX = facingDirection == Vector2.left;
    }

    protected virtual void Tick() { }
    protected virtual void StopMove()
    {
        moveInput = Vector2.zero;
        animator.SetBool("Moving", false);
    }

    public virtual void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
            StopMove();
    }
}
