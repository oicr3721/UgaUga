using UnityEngine;

public class MeleeTeammate : Teammate
{
    [Header("Melee Hitbox")]
    [SerializeField] private Vector2 hitBoxSize = new Vector2(1.2f, 0.8f);
    [SerializeField] private Vector2 hitBoxOffset = new Vector2(0.8f, 0f);
    [SerializeField] private LayerMask targetLayer;

    [Header("Debug")]
    [SerializeField] private bool showAttackDebug = true;
    [SerializeField] private float debugDuration = 0.2f;

    protected override void PerformAttack()
    {
        Vector2 attackDir = facingDirection;
        Vector2 boxCenter = GetHitBoxCenter(attackDir);

        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, hitBoxSize, 0f, targetLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];

            if (hit == null)
                continue;

            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(attackDamage);
            }
        }

        if (showAttackDebug)
        {
            DrawDebugBox(boxCenter, hitBoxSize, debugDuration, Color.red);
        }
    }

    private Vector2 GetHitBoxCenter(Vector2 attackDir)
    {
        // offset.x는 전방, offset.y는 위아래 오프셋이라고 가정
        Vector2 offset = new Vector2(hitBoxOffset.x * attackDir.x, hitBoxOffset.y);
        return (Vector2)transform.position + offset;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showAttackDebug)
            return;

        Vector2 attackDir = facingDirection;
        Vector2 boxCenter = GetHitBoxCenterForGizmo(attackDir);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCenter, hitBoxSize);
    }

    private Vector2 GetHitBoxCenterForGizmo(Vector2 attackDir)
    {
        Vector2 offset = new Vector2(hitBoxOffset.x * attackDir.x, hitBoxOffset.y);
        return (Vector2)transform.position + offset;
    }

    private void DrawDebugBox(Vector2 center, Vector2 size, float duration, Color color)
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