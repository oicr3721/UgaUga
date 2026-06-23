using System.Collections;
using UnityEngine;

public class ArcProjectile : Projectile
{
    [Header("Arc")]
    [SerializeField] private float travelTime = 0.5f;
    [SerializeField] private float arcHeight = 1.5f;
    [SerializeField] private bool rotateAlongPath = false;

    protected override IEnumerator CoMove()
    {
        Vector2 start = transform.position;
        Vector2 end = targetPosition;

        float elapsed = 0f;
        Vector2 prevPos = start;

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            Vector2 nextPos = GetArcPosition(start, end, t);

            RaycastHit2D hit = Physics2D.Linecast(prevPos, nextPos, hitTargetLayer);
            if (hit.collider != null)
            {
                transform.position = hit.point;
                TryHit(hit.collider);
                yield break;
            }

            if (rotateAlongPath)
            {
                Vector2 moveDir = nextPos - prevPos;
                if (moveDir.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }
            }

            transform.position = nextPos;
            prevPos = nextPos;

            yield return null;
        }

        transform.position = end;
        Destroy(gameObject);
    }

    private Vector2 GetArcPosition(Vector2 start, Vector2 end, float t)
    {
        Vector2 pos = Vector2.Lerp(start, end, t);
        pos.y += 4f * arcHeight * t * (1f - t);
        return pos;
    }
}