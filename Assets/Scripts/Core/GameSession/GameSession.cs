using System.Collections.Generic;

public static class GameSession
{
    private static readonly List<TeammateLoadout> Teammates = new();

    public static ObservableValue PlayerMeat { get; private set; }
    public static IReadOnlyList<TeammateLoadout> TeammateLoadouts => Teammates;
    public static int MaxTeammateCount { get; private set; }

    public static void Initialize(
        ObservableValue playerMeat,
        IEnumerable<TeammateData> initialTeammates,
        int maxTeammateCount)
    {
        PlayerMeat = playerMeat;
        MaxTeammateCount = maxTeammateCount;
        SetTeamFromDefaults(initialTeammates);
    }

    public static void SetTeam(IEnumerable<TeammateLoadout> teammates)
    {
        Teammates.Clear();
        Teammates.AddRange(teammates);
    }

    private static void SetTeamFromDefaults(IEnumerable<TeammateData> teammates)
    {
        Teammates.Clear();

        foreach (TeammateData teammate in teammates)
            Teammates.Add(TeammateLoadout.FromDefaultWeapon(teammate));
    }
}
