using UnityEngine;
using UnityEngine.Serialization;

public class RopeProjectile : MonoBehaviour
{
    [Header("Lifetime")]
    [FormerlySerializedAs("lifeTime")]
    [Min(0f)]
    [SerializeField] private float lifetime = 5f;
    [Tooltip("이 월드 Y 좌표보다 아래로 내려가면 투사체를 회수합니다.")]
    [SerializeField] private float minimumWorldY = -1f;

    [Header("Movement")]
    [FormerlySerializedAs("rb")]
    [SerializeField] private Rigidbody2D physicsBody;
    [FormerlySerializedAs("projectileSpeed")]
    [Min(0f)]
    [SerializeField] private float speed = 20f;

    private Rope rope;
    private float elapsedLifetime;

    public void Launch(Rope targetRope, IRopeHoldable holder, Vector2 direction)
    {
        if (targetRope == null)
            return;

        elapsedLifetime = 0f;
        rope = targetRope;
        gameObject.SetActive(true);

        physicsBody.angularVelocity = 0f;
        physicsBody.position = transform.position;
        physicsBody.AddForce(direction.normalized * speed, ForceMode2D.Impulse);

        rope.BeginCast(holder, physicsBody.position);
    }

    private void Update()
    {
        if (rope == null)
            return;

        elapsedLifetime += Time.deltaTime;

        if (elapsedLifetime > lifetime || transform.position.y < minimumWorldY)
        {
            rope.Detach();
            gameObject.SetActive(false);
            return;
        }

        rope.ExtendCast(physicsBody.position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (rope == null || !other.TryGetComponent(out IRopeCatchable catchable))
            return;

        rope.CompleteCast(catchable);
        gameObject.SetActive(false);
    }
}
