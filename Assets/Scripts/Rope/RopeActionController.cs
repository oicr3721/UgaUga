using UnityEngine;
using UnityEngine.InputSystem;

public class RopeActionController : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private Rope rope;
    [SerializeField] private RopeProjectile projectile;
    [SerializeField] private float pullPower = 1f;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    public void RopeThrowInput(InputAction.CallbackContext context)
    {
        if (!context.started)
            return;

        ThrowRope();
    }

    public void RopePullInput(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        rope.Pull(pullPower);
    }

    private void ThrowRope()
    {
        if (cam == null || projectile == null || rope == null)
            return;

        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = Mathf.Abs(cam.transform.position.z);

        Vector3 world = cam.ScreenToWorldPoint(mousePos);
        world.z = 0f;

        Vector2 dir = ((Vector2)(world - transform.position)).normalized;

        projectile.transform.position = transform.position;

        projectile.Launch(rope, player, dir);
    }
}