using System.Collections;
using UnityEngine;

public class Teammate : Character
{
    private static readonly int AttackReadyParameter = Animator.StringToHash("AttackReady");
    private static readonly int AttackParameter = Animator.StringToHash("Attack");

    [Header("Equipment")]
    [SerializeField] private WeaponHolder weaponHolder;

    private Transform currentTarget;
    private Coroutine attackRoutine;
    private float strength;
    private bool isAttackReady;
    private bool isAttacking;
    private bool isCooldown;

    public float Strength => strength;
    public BaseWeapon CurrentWeapon => weaponHolder != null ? weaponHolder.CurrentWeapon : null;
    public float AttackRange => CurrentWeapon != null ? CurrentWeapon.GetAttackRange() : 0f;
    public bool IsAttackReady => isAttackReady;
    public bool IsAttacking => isAttacking;
    public bool IsBusy => isAttackReady || isAttacking;

    public void Initialize(TeammateData data, WeaponData equippedWeapon = null)
    {
        if (data == null)
        {
            Debug.LogError("TeammateData is required to initialize a teammate.", this);
            return;
        }

        moveSpeed = data.MoveSpeed;
        strength = data.Strength;

        if (data.AnimatorController != null)
            animator.runtimeAnimatorController = data.AnimatorController;

        WeaponData weapon = equippedWeapon != null ? equippedWeapon : data.DefaultWeapon;
        EquipWeapon(weapon);
    }

    public bool EquipWeapon(WeaponData weaponData)
    {
        if (weaponHolder != null)
            return weaponHolder.Equip(weaponData, this);

        Debug.LogError("WeaponHolder가 연결되지 않았습니다.", this);
        return false;
    }

    public bool CanAttackTarget(Transform target)
    {
        return target != null && CurrentWeapon != null &&
               !isAttackReady && !isAttacking && !isCooldown;
    }

    public bool Attack(Transform target)
    {
        if (!CanAttackTarget(target))
            return false;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(CoAttackSequence(target));
        return true;
    }

    private IEnumerator CoAttackSequence(Transform target)
    {
        currentTarget = target;
        EnterAttackReady();

        float elapsed = 0f;
        float actualWindup = CurrentWeapon.GetAttackWindup();

        while (elapsed < actualWindup)
        {
            SetMoveInput(Vector2.zero);

            if (currentTarget == null)
            {
                CancelAttackReady();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        StartAttackAnimation();
    }

    private void EnterAttackReady()
    {
        isAttackReady = true;
        animator.SetBool(AttackReadyParameter, true);

        Vector2 direction = currentTarget.position - transform.position;
        Flip(direction);
        CurrentWeapon.BeginAttack(currentTarget);
    }

    private void CancelAttackReady()
    {
        CurrentWeapon.CancelAttack();
        isAttackReady = false;
        currentTarget = null;
        animator.SetBool(AttackReadyParameter, false);
        attackRoutine = null;
    }

    private void StartAttackAnimation()
    {
        isAttackReady = false;
        isAttacking = true;
        animator.SetTrigger(AttackParameter);
        animator.SetBool(AttackReadyParameter, false);
    }

    private void EndAttack()
    {
        CurrentWeapon.EndAttack();
        isAttacking = false;
        StartCoroutine(CoCooldown());
    }

    private IEnumerator CoCooldown()
    {
        isCooldown = true;
        float actualCooldown = CurrentWeapon.GetAttackCooldown();

        yield return new WaitForSeconds(actualCooldown);
        isCooldown = false;
        attackRoutine = null;
    }

    public void Anim_PerformAttack()
    {
        if (!isAttacking || currentTarget == null || CurrentWeapon == null)
            return;

        CurrentWeapon.PerformAttack(currentTarget);
    }

    public void Anim_EndAttack()
    {
        if (isAttacking)
            EndAttack();
    }
}
