using UnityEngine;
using System;

public static class RewardCalculator
{
    public static void Calculate(
        RewardTable rewardTable, float clearTime,
        out ClearGrade grade, out int meatReward)
    {
        foreach (var rule in rewardTable.rewardRules)
        {
            if (clearTime <= rule.maxClearTime)
            {
                grade = rule.grade;
                meatReward = rule.meatReward;

                return;
            }
        }

        RewardRule lastRule = rewardTable.rewardRules[^1];

        grade = lastRule.grade;
        meatReward = lastRule.meatReward;
    }
}