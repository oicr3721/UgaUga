using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveController : MonoBehaviour
{
    private static readonly int MovingParameter = Animator.StringToHash("Moving");

    [Header("Movement Input")]
    [Range(0f, 1f)]
    [SerializeField] private float deadzone = 0.2f;

    [Header("Character")]
    [SerializeField] private Character player;

    private bool isMovementLocked;

    public event Action<Vector2> MoveRequested;

    public void MoveInput(InputAction.CallbackContext context)
    {
        Vector2 rawInput = context.ReadValue<Vector2>();
        if (rawInput.magnitude >= deadzone)
            MoveRequested?.Invoke(rawInput);

        if (isMovementLocked)
            return;

        Vector2 input = rawInput.magnitude < deadzone
            ? Vector2.zero
            : rawInput.normalized;

        if (context.canceled)
            input = Vector2.zero;

        input.y = 0f;
        player.SetMoveInput(input);
    }

    public void LockMovement()
    {
        player.Animator.SetBool(MovingParameter, false);
        isMovementLocked = true;
    }

    public void UnlockMovement()
    {
        isMovementLocked = false;
    }
}
