public static class RewardCalculator
{
    public static void Calculate(
        RewardTable rewardTable,
        float clearTime,
        out ClearGrade grade,
        out int meatReward)
    {
        foreach (RewardRule rule in rewardTable.Rules)
        {
            if (clearTime > rule.MaxClearTime)
                continue;

            grade = rule.Grade;
            meatReward = rule.MeatReward;
            return;
        }

        RewardRule lastRule = rewardTable.Rules[^1];
        grade = lastRule.Grade;
        meatReward = lastRule.MeatReward;
    }
}
