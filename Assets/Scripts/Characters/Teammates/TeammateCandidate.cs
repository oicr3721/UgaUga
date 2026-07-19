using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum CandidateCouncilState
{
    Idle,
    Walking,
    ReachedPosition,
    Rotating,
    Ready
}

public class TeammateCandidate : Character, IInteractable
{
    public static event Action<TeammateCandidate> TeamChangeRequested;

    [Header("Movement")]
    [Min(0f)]
    [SerializeField] private float arriveDistance = 0.05f;

    [Header("Data")]
    [Tooltip("HomeScene 안에서 이 NPC를 식별하는 고유 ID입니다.")]
    [SerializeField] private string candidateId;
    [SerializeField] private TeammateData data;
    [SerializeField] private WeaponData equippedWeapon;

    [Header("World Presentation")]
    [SerializeField] private TeammateDataUI hoverInfo;
    [SerializeField] private SpriteRenderer equippedWeaponRenderer;

    [Header("Weapon Retrieval")]
    [Min(0.05f)]
    [SerializeField] private float weaponHoldDuration = 0.35f;

    private Vector2 originalPosition;
    private Vector2 targetPosition;
    private Transform councilFocus;
    private CandidateCouncilState councilState;
    private bool isCouncilDestination;
    private bool managementEnabled;
    private bool recruited;
    private bool isPointerHeld;
    private bool startedWeaponDrag;
    private float pointerDownTime;
    private TeamManagementView managementView;


    public event Action<TeammateCandidate> BecameReady;

    public string CandidateId => candidateId;
    public TeammateData Data => data;
    public WeaponData EquippedWeapon => equippedWeapon;
    public TeammateLoadout Loadout => new(candidateId, data, equippedWeapon);
    public CandidateCouncilState CouncilState => councilState;
    public bool IsReady => councilState == CandidateCouncilState.Ready;
    public bool IsManagementEnabled => managementEnabled;
    public bool CanInteract => councilState == CandidateCouncilState.Idle;
    public Transform InteractionPoint => transform;
    public string InteractionText => "대화";

    public bool Recruited
    {
        get => recruited;
        set => SetTeamStatus(value);
    }

    private void Awake()
    {
        originalPosition = transform.position;
        councilState = CandidateCouncilState.Idle;
    }

    private void Start()
    {
        animator.Update(UnityEngine.Random.Range(0f, 1f));
        hoverInfo?.UpdateData(data, equippedWeapon);
        hoverInfo?.SetVisible(false);
        UpdateWeaponVisual();
    }

    public void Interact()
    {
        if (CanInteract)
            TeamChangeRequested?.Invoke(this);
    }

    public void SetEquippedWeapon(WeaponData weapon)
    {
        equippedWeapon = weapon;
        hoverInfo?.UpdateData(data, equippedWeapon);
        UpdateWeaponVisual();
    }

    public void SetManagementView(TeamManagementView view)
    {
        managementView = view;
    }

    public void SetTeamStatus(bool isTeamMember)
    {
        recruited = isTeamMember;

        if (IsReady)
            SetSittingState(recruited);
    }

    public void SetManagementEnabled(bool value)
    {
        managementEnabled = value;

        if (!value)
        {
            isPointerHeld = false;
            hoverInfo?.SetVisible(false);
        }
    }

    public void MoveToCouncilSlot(Vector2 position, Transform focus)
    {
        targetPosition = position;
        councilFocus = focus;
        isCouncilDestination = true;
        councilState = CandidateCouncilState.Walking;
        SetSittingState(false);
    }

    public void PlaceAtCouncilSlot(Vector2 position, Transform focus)
    {
        transform.position = position;
        targetPosition = position;
        councilFocus = focus;
        isCouncilDestination = true;
        councilState = CandidateCouncilState.Rotating;
        CompleteRotation();
    }

    public void ReturnHome()
    {
        ClearCouncilSlot();
        targetPosition = originalPosition;
        councilState = CandidateCouncilState.Walking;
        SetSittingState(false);
        SetManagementEnabled(false);
    }

    public void SetDestination(Vector2 position)
    {
        targetPosition = position;
        isCouncilDestination = false;
        councilState = CandidateCouncilState.Walking;
    }

    public void OnHoverEnter()
    {
        if (managementEnabled)
        {
            hoverInfo.SetVisible(true);
            hoverInfo.HoverEnter();
        }
    }

    public void OnHoverExit()
    {
        hoverInfo?.HoverExit();
        hoverInfo?.SetVisible(false);
    }

    protected override void Tick()
    {
        switch (councilState)
        {
            case CandidateCouncilState.Walking:
                UpdateWalking();
                break;
            case CandidateCouncilState.ReachedPosition:
                councilState = CandidateCouncilState.Rotating;
                break;
            case CandidateCouncilState.Rotating:
                CompleteRotation();
                break;
        }

        UpdateWeaponFacing();

        if (isPointerHeld && !startedWeaponDrag && managementEnabled && equippedWeapon != null &&
            Time.unscaledTime - pointerDownTime >= weaponHoldDuration)
        {
            Vector2 pointerPosition = Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : Vector2.zero;
            startedWeaponDrag = managementView != null &&
                managementView.BeginCandidateWeaponDrag(this, pointerPosition);
        }
    }

    private void UpdateWalking()
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;
        if (direction.sqrMagnitude > arriveDistance * arriveDistance)
        {
            SetMoveInput(direction.normalized);
            return;
        }

        transform.position = targetPosition;
        SetMoveInput(Vector2.zero);

        if (isCouncilDestination)
            councilState = CandidateCouncilState.ReachedPosition;
        else
            councilState = CandidateCouncilState.Idle;
    }

    private void CompleteRotation()
    {
        if (councilFocus != null)
            Flip(councilFocus.position - transform.position);

        councilState = CandidateCouncilState.Ready;
        SetSittingState(recruited);
        BecameReady?.Invoke(this);
    }

    private void ClearCouncilSlot()
    {
        councilFocus = null;
        isCouncilDestination = false;
    }

    private void OnMouseEnter()
    {
        OnHoverEnter();
    }

    private void OnMouseExit()
    {
        OnHoverExit();
    }

    private void OnMouseDown()
    {
        if (!managementEnabled)
        {
            Interact();
            return;
        }

        isPointerHeld = true;
        startedWeaponDrag = false;
        pointerDownTime = Time.unscaledTime;
    }

    private void OnMouseUp()
    {
        if (!isPointerHeld)
            return;

        isPointerHeld = false;

        if (!startedWeaponDrag)
            TeamChangeRequested?.Invoke(this);
    }

    private void UpdateWeaponVisual()
    {
        if (equippedWeaponRenderer == null)
            return;

        equippedWeaponRenderer.sprite = equippedWeapon != null ? equippedWeapon.Icon : null;
        equippedWeaponRenderer.enabled = equippedWeaponRenderer.sprite != null;
        UpdateWeaponFacing();
    }

    private void UpdateWeaponFacing()
    {
        if (equippedWeaponRenderer != null && spriteRenderer != null)
            equippedWeaponRenderer.flipX = spriteRenderer.flipX;
    }
}
