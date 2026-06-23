using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : Character, IRopeHoldable
{
    [Header("Component")]
    [SerializeField] private Rigidbody2D rigidBody;

    [Header("Rope")]
    [SerializeField] private Transform attachPoint;
    [SerializeField] private float ropeWeight = 1f;

    [Header("Stamina")]
    [SerializeField] private ObservableValue stamina;
    [SerializeField] private float staminaPerPull = 1f;
    [SerializeField] private float staminaRecoverPerSecond = 2f;
    [SerializeField] private float staminaRecoverDelay = 0.2f;

    private float staminaRecoverTimer;
    private Rope rope;

    public Transform AttachPoint => attachPoint;
    public float RopeWeight => ropeWeight;

    private void Awake()
    {
        stamina?.SetValue(stamina.MaxValue);
    }

    protected override void Tick()
    {
        RecoverStamina();
    }

#region IRopeHoldable
    public void OnRopeHold(Rope rope)
    {
        this.rope = rope;

        animator.SetBool("Catching", true);
    }

    public void OnRopeRelease(Rope rope, Vector2 recoil)
    {
        rigidBody.AddForce(recoil, ForceMode2D.Impulse);
        this.rope = null;

        animator.SetBool("Catching", false);
    }

    public void OnRopeReel(Vector2 force)
    {
        rigidBody.AddForce(force, ForceMode2D.Force);

        staminaRecoverTimer = 0f;
        stamina.SubtractValue(staminaPerPull);

        if (stamina.CurrentValue <= 0f)
        {
            rope.Release();
            this.rope = null;
        }
    }

    public void ApplyRopeForce(Vector2 force)
    {
        rigidBody.AddForce(force, ForceMode2D.Force);
    }
#endregion

    private void RecoverStamina()
    {
        if (stamina == null)
            return;

        staminaRecoverTimer += Time.deltaTime;

        if (staminaRecoverTimer >= staminaRecoverDelay)
        {
            stamina.AddValue(staminaRecoverPerSecond * Time.deltaTime);
        }
    }
}