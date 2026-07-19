using System;
using UnityEngine;

public class Animal : Character, IRopeCatchable, IDamageable
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rigidBody;

    [Header("Stats")]
    [SerializeField] private ObservableValue hp;

    [Header("Rope")]
    [SerializeField] private Transform attachPoint;
    [Min(0.01f)]
    [SerializeField] private float ropeWeight = 3f;

    private Rope rope;

    public ObservableValue HP => hp;
    public Transform AttachPoint => attachPoint;
    public float RopeWeight => ropeWeight;

    public event Action<Animal> Captured;
    public event Action ReachedEndPoint;

    private void Start()
    {
        hp.SetValue(hp.MaxValue);
    }

    public void OnRopeAttached(Rope attachedRope)
    {
        rope = attachedRope;
    }

    public void OnRopeReleased(Rope releasedRope, Vector2 force)
    {
        rigidBody?.AddForce(force);
        rope = null;
    }

    public void OnRopePulled(Vector2 force)
    {
        rigidBody?.AddForce(force);
    }

    public void ApplyRopeForce(Vector2 force)
    {
        force.y = 0f;
        rigidBody?.AddForce(force);
    }

    public void TakeDamage(float damage)
    {
        hp.SubtractValue(damage);
        CheckDeath();
    }

    private void CheckDeath()
    {
        if (hp.CurrentValue > 0f)
            return;

        Captured?.Invoke(this);
        SetCanMove(false);
        rope?.Detach();
        animator.SetTrigger("Dead");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.TryGetComponent<EndPoint>(out _) || HP.CurrentValue <= 0f)
            return;

        ReachedEndPoint?.Invoke();
        rope?.Detach();
    }
}
