using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RigidbodyMover : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private float acceleration = 30f;
    [SerializeField] private float deceleration = 40f;
    [SerializeField] private float maxMoveSpeed = 5f;

    [Header("External")]
    [SerializeField] private float externalDecay = 12f;

    private Vector2 desiredMoveInput;
    private float moveSpeedMultiplier = 1f;

    private Vector2 externalVelocity;
    private Vector2 additiveVelocity;

    public Vector2 Velocity => rigidBody.velocity;
    public Rigidbody2D Rigidbody => rigidBody;
    public Vector2 MoveInput => desiredMoveInput;

    private void Awake()
    {
        rigidBody ??= GetComponent<Rigidbody2D>();
    }

    public void SetMoveInput(Vector2 input)
    {
        desiredMoveInput = Vector2.ClampMagnitude(input, 1f);
    }

    public void SetMoveSpeedMultiplier(float multiplier)
    {
        moveSpeedMultiplier = Mathf.Max(0f, multiplier);
    }

    public void ResetMoveSpeedMultiplier()
    {
        moveSpeedMultiplier = 1f;
    }

    /// <summary>
    /// 순간적인 외력(반동, Pull, Release 등)
    /// </summary>
    public void AddExternalVelocity(Vector2 velocity)
    {
        externalVelocity += velocity;
    }

    /// <summary>
    /// 이번 FixedUpdate에만 적용되는 추가 속도.
    /// Rope의 지속 당김 같은 데 사용.
    /// </summary>
    public void AddFrameVelocity(Vector2 velocity)
    {
        additiveVelocity += velocity;
    }

    /// <summary>
    /// 특정 방향으로 멀어지는 속도를 즉시 잘라낸다.
    /// dir은 "막고 싶은 방향"의 normalized.
    /// </summary>
    public void ClampVelocityAgainst(Vector2 dir, float maxSpeedAlongDir)
    {
        if (dir.sqrMagnitude < 0.0001f)
            return;

        Vector2 velocity = rigidBody.velocity;
        float along = Vector2.Dot(velocity, dir);

        if (along > maxSpeedAlongDir)
        {
            velocity -= dir * (along - maxSpeedAlongDir);
            rigidBody.velocity = velocity;
        }
    }

    private void FixedUpdate()
    {
        Vector2 velocity = rigidBody.velocity;

        // 1) 기본 이동 속도 계산
        Vector2 targetMoveVelocity =
            desiredMoveInput * maxMoveSpeed * moveSpeedMultiplier;

        Vector2 currentMoveLike = velocity - externalVelocity;
        float accel = desiredMoveInput.sqrMagnitude > 0.0001f
            ? acceleration
            : deceleration;

        currentMoveLike = Vector2.MoveTowards(
            currentMoveLike,
            targetMoveVelocity,
            accel * Time.fixedDeltaTime);

        // 2) 외력 + 프레임 당김 적용
        velocity = currentMoveLike + externalVelocity + additiveVelocity;
        rigidBody.velocity = velocity;

        // 3) external 감쇠
        externalVelocity = Vector2.MoveTowards(
            externalVelocity,
            Vector2.zero,
            externalDecay * Time.fixedDeltaTime);

        // 4) 프레임 속도는 1프레임용
        additiveVelocity = Vector2.zero;

        // 5) multiplier는 매 프레임 리셋해두고
        // 필요한 쪽에서 FixedUpdate마다 다시 설정하게 하는 게 안전
        moveSpeedMultiplier = 1f;
    }
}