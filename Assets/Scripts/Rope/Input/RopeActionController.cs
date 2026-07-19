using UnityEngine;
using UnityEngine.InputSystem;

public class RopeActionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Rope rope;
    [SerializeField] private RopeProjectile projectile;

    [Header("Pull")]
    [Min(0f)]
    [SerializeField] private float pullPower = 1f;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void RopeThrowInput(InputAction.CallbackContext context)
    {
        if (context.started)
            ThrowRope();
    }

    public void RopePullInput(InputAction.CallbackContext context)
    {
        if (context.performed)
            rope.Pull(pullPower);
    }

    private void ThrowRope()
    {
        if (mainCamera == null || projectile == null || rope == null)
            return;

        Vector3 mousePosition = Mouse.current.position.ReadValue();
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z);

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0f;

        Vector2 direction =
            ((Vector2)(worldPosition - transform.position)).normalized;

        projectile.transform.position = transform.position;
        projectile.Launch(rope, player, direction);
    }
}
