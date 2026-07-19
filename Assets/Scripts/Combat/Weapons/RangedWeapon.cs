using UnityEngine;

public class RangedWeapon : BaseWeapon
{
    [Header("Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Vector2 aimOffset = Vector2.zero;

    private Projectile loadedProjectile;

    public override float AttackRange => Data.BaseRange + Owner.Strength;

    public override void BeginAttack(Transform target)
    {
        LoadProjectile(target);
    }

    public override void CancelAttack()
    {
        ClearLoadedProjectile();
    }

    public override void PerformAttack(Transform target)
    {
        if (loadedProjectile == null)
            return;

        loadedProjectile.transform.SetParent(null);
        loadedProjectile.Shoot();
        loadedProjectile = null;
    }

    public override void EndAttack()
    {
        ClearLoadedProjectile();
    }

    private void LoadProjectile(Transform target)
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        ClearLoadedProjectile();

        loadedProjectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation, firePoint);
        loadedProjectile.transform.localPosition = Vector3.zero;
        loadedProjectile.transform.localRotation = Quaternion.identity;
        loadedProjectile.Prepare(Damage, Owner.FacingDirection);
        loadedProjectile.SetTargetPosition(GetTargetPosition(target));
    }

    private Vector2 GetTargetPosition(Transform target)
    {
        if (target != null)
        {
            Vector2 targetPosition = target.position;
            targetPosition.x += Random.Range(aimOffset.x, aimOffset.y);

            // 기존 플레이 동작을 보존하기 위해 현재는 오프셋이 적용되지 않은 위치를 사용한다.
            return target.position;
        }

        return (Vector2)firePoint.position + Owner.FacingDirection;
    }

    private void ClearLoadedProjectile()
    {
        loadedProjectile = null;
    }
}
