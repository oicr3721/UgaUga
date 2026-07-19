using System;
using UnityEngine;

public class TeammateCandidate : Character, IInteractable
{
    public static event Action<TeammateCandidate, bool> RecruitmentChanged;

    [Header("Movement")]
    [Min(0f)]
    [SerializeField] private float arriveDistance = 0.05f;

    [Header("Data")]
    [SerializeField] private TeammateData data;
    [SerializeField] private WeaponData equippedWeapon;
    [SerializeField] private TeammateDataUI dataUI;

    private Vector2 targetPosition;
    private bool hasDestination;
    private bool recruited;

    public TeammateData Data
    {
        get => data;
        set
        {
            if (value == null)
                return;

            data = value;
            equippedWeapon = data.DefaultWeapon;
            dataUI.UpdateData(data, equippedWeapon);
        }
    }

    public WeaponData EquippedWeapon => equippedWeapon != null ? equippedWeapon : data.DefaultWeapon;
    public TeammateLoadout Loadout => new(data, EquippedWeapon);
    public bool CanInteract => !hasDestination;
    public Transform InteractionPoint => transform;
    public string InteractionText => recruited ? "제외" : "모집";

    public bool Recruited
    {
        get => recruited;
        set => recruited = value;
    }

    private void Start()
    {
        animator.Update(UnityEngine.Random.Range(0f, 1f));
        dataUI.UpdateData(data, EquippedWeapon);
    }

    public void Interact()
    {
        recruited = !recruited;
        RecruitmentChanged?.Invoke(this, recruited);
    }

    public void SetLoadout(TeammateLoadout loadout)
    {
        data = loadout.Teammate;
        equippedWeapon = loadout.EquippedWeapon;
        dataUI.UpdateData(data, equippedWeapon);
    }

    public void SetDestination(Vector2 position)
    {
        targetPosition = position;
        hasDestination = true;
    }

    public void OnHoverEnter()
    {
        dataUI.HoverEnter();
    }

    public void OnHoverExit()
    {
        dataUI.HoverExit();
    }

    protected override void Tick()
    {
        if (!hasDestination)
        {
            SetMoveInput(Vector2.zero);
            return;
        }

        Vector2 direction = targetPosition - (Vector2)transform.position;

        if (direction.sqrMagnitude <= arriveDistance * arriveDistance)
        {
            transform.position = targetPosition;
            hasDestination = false;
            SetMoveInput(Vector2.zero);
            return;
        }

        SetMoveInput(direction.normalized);
    }
}
