using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    private const float MaximumShare = 1f;
    private const float ShareTolerance = 0.0001f;

    [Header("Scene Candidates")]
    [Tooltip("HomeScene에 미리 배치된 모든 모집 가능 NPC입니다. 런타임에는 생성하지 않습니다.")]
    [SerializeField] private List<TeammateCandidate> candidates = new();

    private readonly List<TeammateCandidate> teamMembers = new();

    public event Action Initialized;
    public event Action TeamChanged;
    public event Action ShareLimitExceeded;

    public IReadOnlyList<TeammateCandidate> AllCandidates => candidates;
    public IReadOnlyList<TeammateCandidate> TeamMembers => teamMembers;
    public float TotalShare => teamMembers.Sum(candidate => candidate.Data.ShareRate);
    public bool IsInitialized { get; private set; }

    public bool IsTeamValid =>
        teamMembers.Count <= GameSession.MaxTeammateCount &&
        TotalShare <= MaximumShare + ShareTolerance;

    private void OnEnable()
    {
        TeammateCandidate.TeamChangeRequested += HandleTeamChangeRequested;
    }

    private void Start()
    {
        ValidateCandidateIds();
        LoadConfirmedTeam();
        IsInitialized = true;
        Initialized?.Invoke();
        TeamChanged?.Invoke();
    }

    private void OnDisable()
    {
        TeammateCandidate.TeamChangeRequested -= HandleTeamChangeRequested;
    }

    public bool TryAddCandidate(TeammateCandidate candidate)
    {
        if (!IsRegisteredCandidate(candidate) || teamMembers.Contains(candidate))
            return false;

        if (teamMembers.Count >= GameSession.MaxTeammateCount)
            return false;

        if (TotalShare + candidate.Data.ShareRate > MaximumShare + ShareTolerance)
        {
            ShareLimitExceeded?.Invoke();
            return false;
        }

        teamMembers.Add(candidate);
        candidate.SetTeamStatus(true);
        NotifyTeamChanged();
        return true;
    }

    public bool RemoveCandidate(TeammateCandidate candidate)
    {
        if (candidate == null || !teamMembers.Remove(candidate))
            return false;

        candidate.SetTeamStatus(false);
        NotifyTeamChanged();
        return true;
    }

    public bool TryEquipWeapon(TeammateCandidate candidate, WeaponData newWeapon)
    {
        if (!IsRegisteredCandidate(candidate) || newWeapon == null)
            return false;

        WeaponData previousWeapon = candidate.EquippedWeapon;
        if (previousWeapon == newWeapon)
            return true;

        if (!GameSession.PlayerWeapons.TryExchange(newWeapon, previousWeapon))
            return false;

        candidate.SetEquippedWeapon(newWeapon);
        SaveCandidateWeapon(candidate, newWeapon);
        TeamChanged?.Invoke();
        return true;
    }

    public bool TryTakeEquippedWeapon(TeammateCandidate candidate, out WeaponData weapon)
    {
        weapon = null;
        if (!IsRegisteredCandidate(candidate) || candidate.EquippedWeapon == null)
            return false;

        weapon = candidate.EquippedWeapon;
        candidate.SetEquippedWeapon(null);
        SaveCandidateWeapon(candidate, null);
        TeamChanged?.Invoke();
        return true;
    }

    public bool EquipTransferredWeapon(TeammateCandidate candidate, WeaponData weapon)
    {
        if (!IsRegisteredCandidate(candidate) || weapon == null)
            return false;

        WeaponData previousWeapon = candidate.EquippedWeapon;
        candidate.SetEquippedWeapon(weapon);
        SaveCandidateWeapon(candidate, weapon);

        if (previousWeapon != null && previousWeapon != weapon)
            GameSession.PlayerWeapons.Add(previousWeapon);

        TeamChanged?.Invoke();
        return true;
    }

    public void ConfirmTeam()
    {
        GameSession.SetTeam(teamMembers.Select(candidate => candidate.Loadout));
    }

    private void HandleTeamChangeRequested(TeammateCandidate candidate)
    {
        if (teamMembers.Contains(candidate))
            RemoveCandidate(candidate);
        else
            TryAddCandidate(candidate);
    }

    private void LoadConfirmedTeam()
    {
        teamMembers.Clear();

        foreach (TeammateCandidate candidate in candidates)
        {
            if (candidate == null)
                continue;

            candidate.SetEquippedWeapon(GameSession.GetCandidateWeapon(candidate.CandidateId));
            candidate.SetTeamStatus(false);
        }

        HashSet<TeammateCandidate> assignedCandidates = new();

        foreach (TeammateLoadout loadout in GameSession.TeammateLoadouts)
        {
            TeammateCandidate candidate = FindCandidate(loadout, assignedCandidates);
            if (candidate == null)
            {
                Debug.LogWarning($"Scene candidate not found for teammate ID '{loadout.Teammate?.TeammateId}'.", this);
                continue;
            }

            candidate.SetEquippedWeapon(loadout.EquippedWeapon);
            SaveCandidateWeapon(candidate, loadout.EquippedWeapon);
            candidate.SetTeamStatus(true);
            teamMembers.Add(candidate);
            assignedCandidates.Add(candidate);
        }

        // Legacy/default loadouts do not yet know a scene candidate ID.
        // Normalize immediately so subsequent scene loads restore the same NPCs.
        GameSession.SetTeam(teamMembers.Select(candidate => candidate.Loadout));
    }

    private TeammateCandidate FindCandidate(
        TeammateLoadout loadout,
        HashSet<TeammateCandidate> assignedCandidates)
    {
        if (loadout?.Teammate == null)
            return null;

        if (!string.IsNullOrWhiteSpace(loadout.CandidateId))
        {
            return candidates.FirstOrDefault(candidate =>
                candidate != null &&
                !assignedCandidates.Contains(candidate) &&
                candidate.CandidateId == loadout.CandidateId);
        }

        string teammateId = loadout.Teammate.TeammateId;
        return candidates.FirstOrDefault(candidate =>
            candidate != null &&
            !assignedCandidates.Contains(candidate) &&
            candidate.Data != null &&
            candidate.Data.TeammateId == teammateId);
    }

    private bool IsRegisteredCandidate(TeammateCandidate candidate)
    {
        return candidate != null && candidates.Contains(candidate);
    }

    private void NotifyTeamChanged()
    {
        TeamChanged?.Invoke();
    }

    private void SaveCandidateWeapon(TeammateCandidate candidate, WeaponData weapon)
    {
        GameSession.SetCandidateWeapon(candidate.CandidateId, weapon);
    }

    private void ValidateCandidateIds()
    {
        HashSet<string> ids = new();

        foreach (TeammateCandidate candidate in candidates)
        {
            if (candidate == null)
                continue;

            if (string.IsNullOrWhiteSpace(candidate.CandidateId))
            {
                Debug.LogError($"Candidate '{candidate.name}' has no candidate ID.", candidate);
                continue;
            }

            if (!ids.Add(candidate.CandidateId))
                Debug.LogError($"Duplicate candidate ID '{candidate.CandidateId}'.", candidate);
        }
    }
}
