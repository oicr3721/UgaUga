
public enum ClearGrade
{
    Fail,
    Bad,
    Normal,
    Good,
    Excellent
}

public class StageResultData
{
    public ClearGrade Grade;
    public float ClearTime;
    public int MeatReward;
    public int TeammateCount;
}

public static class StageResultStorage
{
    public static StageResultData Current { get; private set; }

    public static void Save(StageResultData result)
    {
        Current = result;
    }

    public static void Clear()
    {
        Current = null;
    }
}