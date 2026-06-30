using UnityEngine;

public class Animal : Character, IRopeCatchable, IDamageable
{
    [Header("Component")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private DamageFlash damageFlash;

    [Header("Stat")]
    [SerializeField] private ObservableValue hp;
    public ObservableValue HP => hp;

    [Header("Rope Catchable")]
    [SerializeField] private Transform attachPoint;
    [SerializeField] private float ropeWeight = 3f;

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

    public void OnRopePulled(Vector2 force)
    {
        rigidBody.AddForce(force);
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

        SetCanMove(false);
        Destroy(this);
    }

    public void TakeDamage(float damage)
    {
        hp.SubtractValue(damage);
        damageFlash.Play();

        CheckDeath();
    }
}