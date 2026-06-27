using System.Collections;
using UnityEngine;

public class ArcProjectile : Projectile
{
    [Header("Arc")]
    [SerializeField] private float travelTime = 0.5f;
    [SerializeField] private float arcHeight = 1.5f;

    [Header("Rotation")]
    [SerializeField] private bool rotateAlongPath = true;

    // 스프라이트 기본 방향 보정값
    // 예: 창 스프라이트가 "오른쪽"을 바라보는 상태로 그려졌으면 0
    // 위쪽을 바라보는 상태로 그려졌으면 -90 또는 90 등으로 맞춰주면 됨
    [SerializeField] private float spriteAngleOffset = 0f;

    protected override IEnumerator CoMove()
    {
        Vector2 start = transform.position;
        Vector2 end = targetPosition;

        float elapsed = 0f;
        Vector2 prevPos = start;

        // 시작 시 회전 1회 맞춰주고 싶으면 여기서도 가능
        if (rotateAlongPath)
            UpdateRotation(start, end, 0f);

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);

            Vector2 nextPos = GetArcPosition(start, end, t);

            // 이전 위치 ~ 다음 위치 구간 충돌 체크
            RaycastHit2D hit = Physics2D.Linecast(prevPos, nextPos, hitTargetLayer);
            if (hit.collider != null)
            {
                transform.position = hit.point;

                if (rotateAlongPath)
                {
                    // 충돌 직전 방향으로 마지막 회전 정리
                    Vector2 moveDir = (hit.point - prevPos);
                    if (moveDir.sqrMagnitude > 0.0001f)
                    {
                        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                        transform.rotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffset);
                    }
                }

                TryHit(hit.collider);
                yield break;
            }

            transform.position = nextPos;

            if (rotateAlongPath)
                UpdateRotation(start, end, t);

            prevPos = nextPos;
            yield return null;
        }

        transform.position = end;
        Destroy(gameObject);
    }

    /// <summary>
    /// 포물선 위치 계산
    /// - basePos: start~end 선형 보간
    /// - arcY: 4h t(1-t) 로 중간에서 최고점
    /// </summary>
    private Vector2 GetArcPosition(Vector2 start, Vector2 end, float t)
    {
        Vector2 basePos = Vector2.Lerp(start, end, t);

        // t=0,1 에서 0 / t=0.5에서 arcHeight
        float arcY = 4f * arcHeight * t * (1f - t);

        basePos.y += arcY;
        return basePos;
    }

    /// <summary>
    /// 포물선의 접선 방향으로 회전
    /// 위치 차분(prev->next)이 아니라 수학적 접선 방향을 사용
    /// </summary>
    private void UpdateRotation(Vector2 start, Vector2 end, float t)
    {
        Vector2 tangent = GetArcTangent(start, end, t);

        if (tangent.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + spriteAngleOffset);
    }

    /// <summary>
    /// GetArcPosition의 t에 대한 미분값(접선 벡터)
    ///
    /// position(t) = lerp(start, end, t) + (0, 4h t(1-t))
    ///
    /// d/dt lerp(start, end, t) = end - start
    /// d/dt [4h t(1-t)] = 4h(1 - 2t)
    /// </summary>
    private Vector2 GetArcTangent(Vector2 start, Vector2 end, float t)
    {
        Vector2 linearVelocity = end - start;
        float arcVelocityY = 4f * arcHeight * (1f - 2f * t);

        return new Vector2(
            linearVelocity.x,
            linearVelocity.y + arcVelocityY
        );
    }
}