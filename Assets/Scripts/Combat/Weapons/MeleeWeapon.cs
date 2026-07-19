using UnityEngine;

public class MeleeWeapon : BaseWeapon
{
    [Header("Hitbox")]
    [SerializeField] private Vector2 hitBoxSize = new(1.2f, 0.8f);
    [SerializeField] private Vector2 hitBoxOffset = new(0.8f, 0f);
    [SerializeField] private LayerMask targetLayer;

    [Header("Debug")]
    [SerializeField] private bool showAttackDebug = true;
    [Min(0f)]
    [SerializeField] private float debugDuration = 0.2f;

    public override float Damage => Data.BaseDamage + Owner.Strength;

    public override void PerformAttack(Transform target)
    {
        Vector2 boxCenter = GetHitBoxCenter();
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, hitBoxSize, 0f, targetLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit != null && hit.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(Damage);
        }

        if (showAttackDebug)
            DrawDebugBox(boxCenter, hitBoxSize, debugDuration, Color.red);
    }

    private Vector2 GetHitBoxCenter()
    {
        Vector2 offset = new(hitBoxOffset.x * Owner.FacingDirection.x, hitBoxOffset.y);
        return (Vector2)Owner.transform.position + offset;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showAttackDebug || Owner == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetHitBoxCenter(), hitBoxSize);
    }

    private static void DrawDebugBox(Vector2 center, Vector2 size, float duration, Color color)
    {
        Vector2 half = size * 0.5f;
        Vector2 bottomLeft = center + new Vector2(-half.x, -half.y);
        Vector2 topLeft = center + new Vector2(-half.x, half.y);
        Vector2 topRight = center + new Vector2(half.x, half.y);
        Vector2 bottomRight = center + new Vector2(half.x, -half.y);

        Debug.DrawLine(bottomLeft, topLeft, color, duration);
        Debug.DrawLine(topLeft, topRight, color, duration);
        Debug.DrawLine(topRight, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, bottomLeft, color, duration);
    }
}
