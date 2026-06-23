using System.Collections;
using UnityEngine;

public abstract class Teammate : Character
{
    [Header("Attack")]
    [SerializeField] protected float attackDamage = 3f;
    [SerializeField] protected float attackRange = 1f;

    [Header("Attack Timing")]
    [SerializeField] protected float attackWindup = 0.25f;   // AttackReady 유지 시간
    [SerializeField] protected float attackCooldown = 0.5f;  // 공격 종료 후 다음 행동까지 대기

    protected Transform currentTarget;

    protected bool isAttackReady;
    protected bool isAttacking;
    protected bool isCooldown;

    protected Coroutine attackRoutine;

    public float AttackRange => attackRange;
    public bool IsAttackReady => isAttackReady;
    public bool IsAttacking => isAttacking;
    public bool IsBusy => isAttackReady || isAttacking;

    public bool CanAttackTarget(Transform target)
    {
        if (target == null)
            return false;

        if (isAttackReady || isAttacking || isCooldown)
            return false;

        return true;
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

    protected virtual IEnumerator CoAttackSequence(Transform target)
    {
        currentTarget = target;

        // 1) Ready 시작
        EnterAttackReady();

        float elapsed = 0f;
        while (elapsed < attackWindup)
        {
            // Ready 중에는 이동 금지
            SetMoveInput(Vector2.zero);

            // 필요하면 여기서 타겟 상실/사거리 이탈 시 취소 가능
            if (currentTarget == null)
            {
                CancelAttackReady();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2) 실제 공격 시작
        StartAttackAnimation();
    }

    protected virtual void EnterAttackReady()
    {
        isAttackReady = true;
        animator.SetBool("AttackReady", true);

        Vector2 dir = currentTarget.transform.position - transform.position;
        Flip(dir);
    }

    protected virtual void CancelAttackReady()
    {
        Debug.Log("Cancle Attack Ready");
        isAttackReady = false;
        currentTarget = null;
        animator.SetBool("AttackReady", false);
        attackRoutine = null;
    }

    protected virtual void StartAttackAnimation()
    {
        isAttackReady = false;
        isAttacking = true;

        animator.SetTrigger("Attack");
        animator.SetBool("AttackReady", false);
    }

    protected virtual void EndAttack()
    {
        isAttacking = false;
        StartCoroutine(CoCooldown());
    }

    protected virtual IEnumerator CoCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        isCooldown = false;
        attackRoutine = null;
    }

    // -----------------------------
    // Animation Events (공통)
    // -----------------------------

    /// <summary>
    /// 공격 애니메이션의 실질적 공격 시점에서 호출
    /// Melee = 타격 판정
    /// Ranged = 발사
    /// </summary>
    public void Anim_PerformAttack()
    {
        if (!isAttacking)
            return;

        if (currentTarget == null)
            return;

        PerformAttack();
    }

    /// <summary>
    /// 공격 애니메이션 종료 시점에서 호출
    /// </summary>
    public void Anim_EndAttack()
    {
        EndAttack();
    }

    /// <summary>
    /// 실제 공격 구현은 하위 클래스가 담당
    /// </summary>
    protected abstract void PerformAttack();
}