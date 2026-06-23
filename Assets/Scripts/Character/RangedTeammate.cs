using UnityEngine;

public class RangedTeammate : Teammate
{
    [Header("Ranged")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;

    private Projectile loadedProjectile;

    protected override void EnterAttackReady()
    {
        base.EnterAttackReady();
        LoadProjectile();
    }

    protected override void CancelAttackReady()
    {
        ClearLoadedProjectile();
        base.CancelAttackReady();
    }

    protected override void PerformAttack()
    {
        if (loadedProjectile == null)
            return;

        loadedProjectile.SetTargetPosition(GetTargetPosition());
        loadedProjectile.transform.SetParent(null);
        loadedProjectile.Shoot();
        loadedProjectile = null;
    }

    protected override void EndAttack()
    {
        ClearLoadedProjectile();
        base.EndAttack();
    }

    private void LoadProjectile()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        ClearLoadedProjectile();

        loadedProjectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        loadedProjectile.transform.SetParent(firePoint);
        loadedProjectile.transform.localPosition = Vector3.zero;
        loadedProjectile.transform.localRotation = Quaternion.identity;

        loadedProjectile.Prepare(attackDamage, facingDirection);
        loadedProjectile.SetTargetPosition(GetTargetPosition());
    }

    private Vector2 GetTargetPosition()
    {
        if (currentTarget != null)
            return currentTarget.position;

        return (Vector2)firePoint.position + facingDirection;
    }

    private void ClearLoadedProjectile()
    {
        loadedProjectile = null;
    }
}