using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct RewardRule
{
    [FormerlySerializedAs("grade")]
    [SerializeField] private ClearGrade grade;
    [FormerlySerializedAs("maxClearTime")]
    [Min(0f)]
    [SerializeField] private float maxClearTime;
    [FormerlySerializedAs("meatReward")]
    [Min(0)]
    [SerializeField] private int meatReward;

    public ClearGrade Grade => grade;
    public float MaxClearTime => maxClearTime;
    public int MeatReward => meatReward;
}

[CreateAssetMenu(fileName = "RewardTable", menuName = "Game/Reward Table")]
public class RewardTable : ScriptableObject
{
    [FormerlySerializedAs("rewardRules")]
    [SerializeField] private RewardRule[] rules;

    public RewardRule[] Rules => rules;
}
