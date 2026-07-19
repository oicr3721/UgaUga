using System.Collections;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] protected float damage = 1f;
    [SerializeField] protected LayerMask hitTargetLayer;
    [SerializeField] protected bool destroyOnHit = true;

    protected Vector2 targetPosition;
    protected bool isShot;
    protected bool hasHit;

    public virtual void Prepare(float damage, Vector2 facingDir)
    {
        this.damage = damage;
        isShot = false;
        hasHit = false;

        transform.localScale = new Vector3(facingDir.x, 1, 1);
    }

    public virtual void SetTargetPosition(Vector2 targetPosition)
    {
        this.targetPosition = targetPosition;
    }

    public void Shoot()
    {
        if (isShot)
            return;

        isShot = true;
        StartCoroutine(CoMove());
    }

    protected abstract IEnumerator CoMove();

    protected bool TryHit(Collider2D col)
    {
        if (hasHit || col == null)
            return false;

        if ((hitTargetLayer.value & (1 << col.gameObject.layer)) == 0)
            return false;

        hasHit = true;

        if (col.TryGetComponent<IDamageable>(out var damageable))
            damageable.TakeDamage(damage);

        if (destroyOnHit)
            Destroy(gameObject);

        return true;
    }
}