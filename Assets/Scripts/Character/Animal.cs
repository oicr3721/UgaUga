using UnityEngine;

public class Animal : Character, IRopeCatchable, IDamageable
{
    [Header("Component")]
    [SerializeField] private Rigidbody2D rigidBody;

    [Header("Stat")]
    [SerializeField] private ObservableValue hp;
    public ObservableValue HP => hp;

    [Header("Rope Catchable")]
    [SerializeField] private Transform attachPoint;
    [SerializeField] private float ropeWeight = 3f;
    [SerializeField] private float pulledDamage = 1f;
    [SerializeField] private float pulledPower = 2f;

    public Transform AttachPoint => attachPoint;
    public float RopeWeight => ropeWeight;

    private void Start()
    {
        hp.SetValue(hp.MaxValue);
    }

    #region IRopeCatchable
    public void OnRopeAttached(Rope rope)
    {

    }

    public void OnRopeReleased(Rope rope, Vector2 force)
    {
        rigidBody.AddForce(force);
    }

    public void OnRopePulled(Vector2 dir)
    {
        hp.SubtractValue(pulledDamage);

        CheckDeath();

        rigidBody.AddForce(pulledPower * dir);
    }

    public void ApplyRopeForce(Vector2 force)
    {
        force.y = 0;
        rigidBody.AddForce(force);
    }
    #endregion

    private void CheckDeath()
    {
        if (hp.CurrentValue > 0f)
            return;

        HuntingStageManager.Instance?.OnAnimalCaptured(this);
    }

    public void TakeDamage(float damage)
    {
        hp.SubtractValue(damage);
    }
}