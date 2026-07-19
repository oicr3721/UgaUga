using UnityEngine;

public class Character : MonoBehaviour
{
    private static readonly int MovingParameter = Animator.StringToHash("Moving");

    [Header("Visual")]
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected Animator animator;

    [Header("Movement")]
    [Min(0f)]
    [SerializeField] protected float moveSpeed = 1f;

    protected Vector2 facingDirection = Vector2.right;

    private Vector2 moveInput;
    private bool canMove = true;

    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Animator Animator => animator;
    public Vector2 FacingDirection => facingDirection;

    private void Update()
    {
        Tick();

        if (!canMove || moveInput == Vector2.zero)
            return;

        transform.position += (Vector3)moveInput * moveSpeed * Time.deltaTime;
    }

    public void SetMoveInput(Vector2 input)
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = input;
        animator.SetBool(MovingParameter, input != Vector2.zero);
        Flip(input);
    }

    public void Flip(Vector2 direction)
    {
        if (direction.x == 0f || spriteRenderer == null)
            return;

        facingDirection = direction.x < 0f
            ? Vector2.left
            : Vector2.right;

        spriteRenderer.flipX = facingDirection == Vector2.left;
    }

    public virtual void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
            StopMove();
    }

    public virtual void SetSittingState(bool isSitting)
    {
        // TODO: 임시 구현. Sit 전용 Animator Parameter 추가 후 교체한다.
        animator.SetBool(MovingParameter, isSitting);
    }

    protected virtual void Tick()
    {
    }

    protected virtual void StopMove()
    {
        moveInput = Vector2.zero;
        animator.SetBool(MovingParameter, false);
    }
}
