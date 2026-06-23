using UnityEngine;

public class RopeProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float projectileSpeed = 20f;

    private Rope rope;
    private float lt;

    public void Launch(Rope rope, IRopeHoldable holder, Vector2 dir)
    {
        if (rope == null) return;

        lt = 0f;
        this.rope = rope;

        gameObject.SetActive(true);

        rb.angularVelocity = 0f;
        rb.position = transform.position;

        rb.AddForce(dir.normalized * projectileSpeed, ForceMode2D.Impulse);

        rope.BeginCast(holder, rb.position);
    }

    private void Update()
    {
        if (rope == null) return;

        lt += Time.deltaTime;

        if (lt > lifeTime || transform.position.y < -1)
        {
            rope.ResetRope();
            gameObject.SetActive(false);
            return;
        }

        rope.ExtendCast(rb.position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (rope == null) return;

        if (!other.TryGetComponent<IRopeCatchable>(out var catchable))
            return;

        rope.CompleteCast(catchable);
        gameObject.SetActive(false);
    }
}