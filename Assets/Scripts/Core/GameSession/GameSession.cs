using System.Collections.Generic;

public static class GameSession
{
    private static readonly List<TeammateLoadout> Teammates = new();
    private static readonly WeaponInventory Weapons = new();
    private static readonly Dictionary<string, WeaponData> CandidateWeapons = new();

    public static ObservableValue PlayerMeat { get; private set; }
    public static IReadOnlyList<TeammateLoadout> TeammateLoadouts => Teammates;
    public static WeaponInventory PlayerWeapons => Weapons;
    public static int MaxTeammateCount { get; private set; }

    public static void Initialize(
        ObservableValue playerMeat,
        IEnumerable<TeammateData> initialTeammates,
        IEnumerable<WeaponStack> initialWeapons,
        int maxTeammateCount)
    {
        PlayerMeat = playerMeat;
        MaxTeammateCount = maxTeammateCount;
        SetTeamFromDefaults(initialTeammates);
        Weapons.Initialize(initialWeapons);
    }

    public static void SetTeam(IEnumerable<TeammateLoadout> teammates)
    {
        Teammates.Clear();
        Teammates.AddRange(teammates);
    }

    public static WeaponData GetCandidateWeapon(string candidateId)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
            return null;

        return CandidateWeapons.TryGetValue(candidateId, out WeaponData weapon) ? weapon : null;
    }

    public static void SetCandidateWeapon(string candidateId, WeaponData weapon)
    {
        if (string.IsNullOrWhiteSpace(candidateId))
            return;

        if (weapon == null)
            CandidateWeapons.Remove(candidateId);
        else
            CandidateWeapons[candidateId] = weapon;
    }

    private static void SetTeamFromDefaults(IEnumerable<TeammateData> teammates)
    {
        Teammates.Clear();
        CandidateWeapons.Clear();

        foreach (TeammateData teammate in teammates)
        {
            TeammateLoadout loadout = TeammateLoadout.FromDefaultWeapon(teammate);
            Teammates.Add(loadout);
        }
    }
}
