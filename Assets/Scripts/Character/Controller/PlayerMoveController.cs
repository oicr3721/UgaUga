using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoveController : MonoBehaviour
{
    [Header("이동 입력 데드존")]
    [SerializeField] private float deadzone = 0.2f;

    [Header("Character")]
    [SerializeField] private Character player;

    private bool isMovementLocked = false;

    public void MoveInput(InputAction.CallbackContext context)
    {
        if (isMovementLocked)
            return;

        Vector2 raw = context.ReadValue<Vector2>();

        Vector2 input =
            raw.magnitude < deadzone
            ? Vector2.zero
            : raw.normalized;

        if (context.canceled)
            input = Vector2.zero;

        input.y = 0;

        player.SetMoveInput(input);
    }

    public void LockMovement()
    {
        player.Animator.SetBool(
            "Moving",
            false
        );

        isMovementLocked = true;
    }

    public void UnLockMovement()
    {
        isMovementLocked = false;
    }
}