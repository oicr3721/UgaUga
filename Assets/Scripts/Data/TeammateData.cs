using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeammateType
{
    Melee,
    Ranged
}

[CreateAssetMenu(fileName = "Teammate Data", menuName = "Game/Teammate Data")]
public class TeammateData : ScriptableObject
{
    public TeammateType type;

    [Header("Attack")]
    public float attackDamage;
    public float attackRange;

    [Header("Attack Timing")]
    public float attackWindup;   // AttackReady 유지 시간
    public float attackCooldown;  // 공격 종료 후 다음 행동까지 대기

    [Header("Offset")]
    public Vector2 windupOffset;
    public Vector2 cooldownOffset;
    public Vector2 attackRangeOffset;
}
