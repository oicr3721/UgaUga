using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class TeamManager : MonoBehaviour
{
    [Header("Candidates")]
    [SerializeField] private List<TeammateCandidate> candidates = new();
    [SerializeField] private TeammateCandidate candidatePrefab;

    [Header("Lineup")]
    [Tooltip("출구 앞에 후보를 배치할 때 사용하는 간격입니다.")]
    [Min(0.01f)]
    [SerializeField] private float lineSpace;
    [SerializeField] private Transform exitTransform;
    [SerializeField] private Transform cutlineTransform;
    [SerializeField] private Vector2 waitPosRange = new(3f, 8f);

    [Header("Cost")]
    [FormerlySerializedAs("RequiredMeat")]
    [SerializeField] private ObservableValue requiredMeat;

    private CandidateLineup lineup;

    public ObservableValue RequiredMeat => requiredMeat;

    public bool IsTeamValid =>
        candidates.Count <= GameSession.MaxTeammateCount &&
        requiredMeat.CurrentValue <= GameSession.PlayerMeat.CurrentValue;

    private void Awake()
    {
        lineup = new CandidateLineup(exitTransform, cutlineTransform, lineSpace, waitPosRange);
    }

    private void OnEnable()
    {
        TeammateCandidate.RecruitmentChanged += HandleRecruitmentChanged;
    }

    private void Start()
    {
        LoadConfirmedTeam();
        lineup.SetCutline(GameSession.MaxTeammateCount);
    }

    private void OnDisable()
    {
        TeammateCandidate.RecruitmentChanged -= HandleRecruitmentChanged;
    }

    private void HandleRecruitmentChanged(TeammateCandidate candidate, bool recruited)
    {
        if (recruited)
            AddCandidate(candidate);
        else
            RemoveCandidate(candidate);
    }

    private void AddCandidate(TeammateCandidate candidate)
    {
        if (candidates.Contains(candidate))
            return;

        candidates.Add(candidate);
        UpdateCandidateLineup();
        UpdateRequiredMeat();
    }

    private void RemoveCandidate(TeammateCandidate candidate)
    {
        candidates.Remove(candidate);
        lineup.MoveToWaitingPosition(candidate);
        UpdateCandidateLineup();
        UpdateRequiredMeat();
    }

    public void ConfirmTeam()
    {
        GameSession.PlayerMeat.SubtractValue(requiredMeat.CurrentValue);
        GameSession.SetTeam(candidates.Select(candidate => candidate.Loadout));
    }

    private void LoadConfirmedTeam()
    {
        candidates.Clear();

        for (int i = 0; i < GameSession.TeammateLoadouts.Count; i++)
        {
            TeammateCandidate candidate = Instantiate(candidatePrefab);
            candidate.SetLoadout(GameSession.TeammateLoadouts[i]);
            candidate.Recruited = true;
            candidates.Add(candidate);
            lineup.PlaceImmediately(candidate, i);
        }

        UpdateRequiredMeat();
    }

    private void UpdateCandidateLineup()
    {
        lineup.Arrange(candidates);
    }

    private void UpdateRequiredMeat()
    {
        int totalCost = candidates.Sum(candidate => candidate.Data.MeatCost);
        requiredMeat.SetValue(totalCost);
    }
}
