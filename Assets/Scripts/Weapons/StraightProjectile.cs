using System.Collections;
using UnityEngine;

public class StraightProjectile : Projectile
{
    [Header("Straight")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float maxLifeTime = 3f;
    [SerializeField] private bool rotateToMoveDirection = true;

    protected override IEnumerator CoMove()
    {
        Vector2 current = transform.position;
        Vector2 dir = (targetPosition - current).normalized;

        if (dir.sqrMagnitude <= 0.0001f)
        {
            Destroy(gameObject);
            yield break;
        }

        if (rotateToMoveDirection)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        float elapsed = 0f;

        while (elapsed < maxLifeTime)
        {
            float moveDist = speed * Time.deltaTime;

            Vector2 prevPos = transform.position;
            Vector2 nextPos = prevPos + dir * moveDist;

            RaycastHit2D hit = Physics2D.Linecast(prevPos, nextPos, hitTargetLayer);
            if (hit.collider != null)
            {
                transform.position = hit.point;
                TryHit(hit.collider);
                yield break;
            }

            transform.position = nextPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}