using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CampfireCouncilState
{
    Normal,
    Gathering,
    TeamManagement
}

public class CampfireCouncilController : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private TeamManager teamManager;
    [SerializeField] private TeamManagementView managementView;
    [SerializeField] private Character player;
    [SerializeField] private PlayerMoveController playerMoveController;
    [SerializeField] private Transform campfire;
    [SerializeField] private Transform playerSeat;

    [Header("Slot Layout")]
    [Min(0.1f)]
    [SerializeField] private float firstSlotDistance = 0.9f;
    [Min(0.1f)]
    [SerializeField] private float slotSpacing = 0.75f;
    [SerializeField] private float slotVerticalOffset;

    [Header("Gathering")]
    [Min(0.01f)]
    [SerializeField] private float playerArriveDistance = 0.05f;

    private CampfireSlotLayout slotLayout;
    private bool playerReady;

    public CampfireCouncilState State { get; private set; }
    public bool CanStartCouncil => State == CampfireCouncilState.Normal && teamManager.IsInitialized;

    private void Awake()
    {
        slotLayout = new CampfireSlotLayout(firstSlotDistance, slotSpacing, slotVerticalOffset);
        State = CampfireCouncilState.Normal;
        managementView.SetVisible(false);
    }

    private void OnEnable()
    {
        teamManager.Initialized += HandleTeamManagerInitialized;
        teamManager.TeamChanged += HandleTeamChanged;
        playerMoveController.MoveRequested += HandlePlayerMoveRequested;
    }

    private void Start()
    {
        managementView.Bind(teamManager);

        if (teamManager.IsInitialized)
            HandleTeamManagerInitialized();
    }

    private void OnDisable()
    {
        teamManager.Initialized -= HandleTeamManagerInitialized;
        teamManager.TeamChanged -= HandleTeamChanged;
        playerMoveController.MoveRequested -= HandlePlayerMoveRequested;
    }

    private void Update()
    {
        if (State != CampfireCouncilState.Gathering)
            return;

        UpdatePlayerGatheringMovement();

        if (playerReady && AreAllCandidatesReady())
            EnterTeamManagement();
    }

    public void BeginCouncil()
    {
        if (!CanStartCouncil)
            return;

        State = CampfireCouncilState.Gathering;
        playerReady = false;
        playerMoveController.LockMovement();
        player.SetSittingState(false);

        IReadOnlyList<TeammateCandidate> candidates = teamManager.AllCandidates;
        IReadOnlyList<Vector2> slots = slotLayout.CreateSlots(campfire.position, candidates.Count);

        for (int i = 0; i < candidates.Count; i++)
        {
            candidates[i].SetManagementEnabled(false);
            candidates[i].MoveToCouncilSlot(slots[i], campfire);
        }
    }

    public void ExitCouncil()
    {
        if (State != CampfireCouncilState.TeamManagement)
            return;

        managementView.SetVisible(false);
        player.SetSittingState(false);
        player.SetMoveInput(Vector2.zero);
        playerMoveController.UnlockMovement();

        ArrangeRestingTeamMembers();
        State = CampfireCouncilState.Normal;
    }

    private void HandleTeamManagerInitialized()
    {
        ArrangeRestingTeamMembers();
    }

    private void HandleTeamChanged()
    {
        if (State == CampfireCouncilState.TeamManagement)
            managementView.Refresh();
    }

    private void HandlePlayerMoveRequested(Vector2 input)
    {
        if (State == CampfireCouncilState.TeamManagement && input.sqrMagnitude > 0f)
            ExitCouncil();
    }

    private void UpdatePlayerGatheringMovement()
    {
        if (playerReady)
            return;

        Vector2 direction = playerSeat.position - player.transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > playerArriveDistance * playerArriveDistance)
        {
            player.SetMoveInput(direction.normalized);
            return;
        }

        player.transform.position = new Vector3(
            playerSeat.position.x,
            player.transform.position.y,
            player.transform.position.z);
        player.SetMoveInput(Vector2.zero);
        player.Flip(campfire.position - player.transform.position);
        playerReady = true;
    }

    private bool AreAllCandidatesReady()
    {
        foreach (TeammateCandidate candidate in teamManager.AllCandidates)
        {
            if (!candidate.IsReady)
                return false;
        }

        return true;
    }

    private void EnterTeamManagement()
    {
        State = CampfireCouncilState.TeamManagement;
        player.SetSittingState(true);

        foreach (TeammateCandidate candidate in teamManager.AllCandidates)
        {
            candidate.SetManagementView(managementView);
            candidate.SetManagementEnabled(true);
        }

        managementView.Refresh();
        managementView.SetVisible(true);
    }

    private void ArrangeRestingTeamMembers()
    {
        IReadOnlyList<TeammateCandidate> teamMembers = teamManager.TeamMembers;
        IReadOnlyList<Vector2> slots = slotLayout.CreateSlots(campfire.position, teamMembers.Count);

        for (int i = 0; i < teamMembers.Count; i++)
        {
            TeammateCandidate member = teamMembers[i];
            member.SetManagementEnabled(false);
            member.PlaceAtCouncilSlot(slots[i], campfire);
            member.SetTeamStatus(true);
        }

        foreach (TeammateCandidate candidate in teamManager.AllCandidates)
        {
            if (!teamMembers.Contains(candidate))
                candidate.ReturnHome();
        }
    }
}
