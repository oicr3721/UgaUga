using UnityEngine;

public class TeammateController : MonoBehaviour
{
    [SerializeField] private Teammate teammate;
    [SerializeField] private Transform attackTarget;

    [Header("Combat Distance")]
    [SerializeField] private float distanceTolerance = 0.8f;

    private void OnDisable()
    {
        teammate.SetCanMove(false);
    }

    private void OnEnable()
    {
        teammate.SetCanMove(true);
    }

    private void Update()
    {
        if (teammate == null || attackTarget == null)
            return;

        if (teammate.IsAttacking || teammate.IsAttackReady)
        {
            teammate.SetMoveInput(Vector2.zero);
            return;
        }

        float dist = Vector2.Distance(
            teammate.transform.position,
            attackTarget.position
        );

        // -------------------------
        // 1) 너무 멀다 → 접근
        // -------------------------
        if (dist > teammate.AttackRange + distanceTolerance)
        {
            Vector2 moveDir =
                (attackTarget.position - teammate.transform.position).normalized;

            moveDir.y = 0;
            teammate.SetMoveInput(moveDir);
            return;
        }

        // -------------------------
        // 2) 너무 가깝다 → 후퇴
        // -------------------------
        if (dist < teammate.AttackRange - distanceTolerance)
        {
            Vector2 moveDir =
                (teammate.transform.position - attackTarget.position).normalized;

            moveDir.y = 0;
            teammate.SetMoveInput(moveDir);
            return;
        }

        // -------------------------
        // 3) 적정 거리 → 공격
        // -------------------------
        teammate.SetMoveInput(Vector2.zero);
        teammate.Attack(attackTarget);
    }

    public void SetTarget(Transform target)
    {
        attackTarget = target;
    }
}