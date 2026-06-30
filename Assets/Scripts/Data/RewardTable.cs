using System;
using UnityEngine;


[Serializable]
public struct RewardRule
{
    public ClearGrade grade;
    public float maxClearTime;
    public int meatReward;
}

[CreateAssetMenu(fileName = "RewardTable", menuName = "Game/Reward Table")]
public class RewardTable : ScriptableObject
{
    public RewardRule[] rewardRules;
}