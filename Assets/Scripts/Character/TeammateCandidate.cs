using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeammateCandidate : Character, IInteractable
{

    [SerializeField] private float arriveDistance = 0.05f;
    [SerializeField] private TeammateDataUI dataUI;

    private Vector2 targetPosition;
    private bool hasDestination;

    [SerializeField] private TeammateData data;
    public TeammateData Data
    {
        get
        {
            return data;
        }
        set
        {
            if (value == null) return;
            data = value;
            dataUI.UpdateData(data);
        }
    }


    public bool CanInteract => !hasDestination;

    public Transform InteractionPoint => transform;

    public string InteractionText => recruited ? "퇴출" : "모집";

    private bool recruited = false;

    private void Start()
    {
        dataUI.UpdateData(data);
    }

    public void Interact()
    {
        recruited = !recruited;

        if(recruited)
            TeamManager.Instance.AddCandidate(this);
        else
            TeamManager.Instance.RemoveCandidate(this);
    }

    public void SetDestination(Vector2 position)
    {
        targetPosition = position;
        hasDestination = true;
    }

    protected override void Tick()
    {
        if (!hasDestination)
        {
            SetMoveInput(Vector2.zero);
            return;
        }

        Vector2 dir = targetPosition - (Vector2)transform.position;

        if (dir.sqrMagnitude <= arriveDistance * arriveDistance)
        {
            transform.position = targetPosition;
            hasDestination = false;
            SetMoveInput(Vector2.zero);
            return;
        }

        SetMoveInput(dir.normalized);
    }

    public void OnHoverEnter()
    {
        dataUI.HoverEnter();
    }

    public void OnHoverExit()
    {
        dataUI.HoverExit();
    }
}
