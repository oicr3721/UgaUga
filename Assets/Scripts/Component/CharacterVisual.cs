using UnityEngine;

public class CharacterVisual : MonoBehaviour
{
    [SerializeField] private RigidbodyMover mover;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Visual")]
    [SerializeField] private float movingThreshold = 0.01f;

    private bool facingLeft;

    private void Awake()
    {
        mover ??= GetComponent<RigidbodyMover>();
        animator ??= GetComponent<Animator>();
        spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        UpdateMoving();
        UpdateFacing();
    }

    private void UpdateMoving()
    {
        bool moving = Mathf.Abs(mover.MoveInput.x) > movingThreshold;
        animator.SetBool("Moving", moving);
    }

    private void UpdateFacing()
    {
        float x = mover.MoveInput.x;

        if (Mathf.Abs(x) <= movingThreshold)
            return;

        bool nextFacingLeft = x < 0f;

        if (nextFacingLeft == facingLeft)
            return;

        facingLeft = nextFacingLeft;

        spriteRenderer.flipX = facingLeft;
    }
}