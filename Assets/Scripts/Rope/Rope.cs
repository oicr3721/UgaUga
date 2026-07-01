using Cinemachine;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    private IRopeHoldable holder;
    private IRopeCatchable target;

    private float ropeLength;

    [Header("Verlet")]
    [SerializeField] private float pointSpacing = 0.05f;
    [SerializeField] private float pullDistance = 1f;
    [SerializeField] private float tensionForceMultiplier = 30f;
    [SerializeField] private int constraintIterations = 10;
    [SerializeField] private float ropeGravity = -1f;

    [Header("Durability")]
    [SerializeField] private ObservableValue durability;
    [SerializeField] private float durabilityLossPerTension = 1.5f;
    [SerializeField] private float durabilityRecoverPerSecond = 1f;
    [SerializeField] private float recoverDelay = 1.2f;
    [SerializeField] private float dangerDurabilityMultiplier = 2f;

    [Header("Rope")]
    [SerializeField] private Transform midPoint;
    [SerializeField] private float releaseForce = 0.5f;
    [SerializeField] private float pullForce = 1.2f;
    [SerializeField] private float loosenLength = 0.02f;

    [Header("Tension")]
    [SerializeField] private float tensionThreshold = 0.1f;
    [SerializeField] private float dangerTension = 2f;

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera ropeVirtualCamera;

    public float CurrentTension { get; private set; }
    public float DangerTension => dangerTension;

    private float noTensionTimer;

    public float CurrentDurability => durability.CurrentValue;
    public float MaxDurability => durability.MaxValue;

    private readonly List<RopePoint> points = new();
    public List<RopePoint> Points => points;

    private float segmentLength;
    private Vector2 castTipPosition;

    public event Action<Vector2> OnPullDirection;
    public event Action OnReset;

    public enum RopeState
    {
        Idle,
        Casting,
        Attached
    }

    public RopeState State { get; private set; } = RopeState.Idle;

    private void Awake()
    {
        durability.SetValue(durability.MaxValue);
    }

    #region Verlet
    public void BeginCast(IRopeHoldable holdable, Vector2 startPosition)
    {
        Release();

        if (holdable == null)
            return;

        holder = holdable;
        target = null;

        points.Clear();
        points.Add(new RopePoint(startPosition));

        ropeLength = 0f;
        segmentLength = 0f;
        castTipPosition = startPosition;

        State = RopeState.Casting;
    }

    public void ExtendCast(Vector2 tipPosition)
    {
        if (State != RopeState.Casting || points.Count == 0)
            return;

        castTipPosition = tipPosition;

        float dist = Vector2.Distance(points[^1].Position, tipPosition);

        if (dist < pointSpacing)
            return;

        RopePoint prev = points[^1];
        prev.PreviousPosition = prev.Position;

        points.Add(new RopePoint(tipPosition));

        ropeLength += dist;
        segmentLength = points.Count > 1 ? ropeLength / (points.Count - 1) : 0f;
    }

    public void CompleteCast(IRopeCatchable catchable)
    {
        if (State != RopeState.Casting || catchable == null || points.Count == 0)
            return;

        target = catchable;

        RopePoint prev = points[^1];
        prev.PreviousPosition = prev.Position;

        Vector2 finalPos = target.AttachPoint.position;

        ropeLength += Vector2.Distance(points[^1].Position, finalPos);
        points.Add(new RopePoint(finalPos));

        segmentLength = points.Count > 1 ? ropeLength / (points.Count - 1) : 0f;

        holder?.OnRopeHold(this);
        target?.OnRopeAttached(this);
        State = RopeState.Attached;

        EnableRopeCamera();
    }
    #endregion

    public void Release()
    {
       //State = RopeState.Idle;

        if (target == null || holder == null)
        {
            ResetRope();
            holder = null;
            target = null;
            return;
        }

        Vector2 force = GetTensionForce();

        CalculateDistributedForce(
            force,
            out Vector2 holderForce,
            out Vector2 targetForce);

        holder?.OnRopeRelease(this, -holderForce * releaseForce);
        target?.OnRopeReleased(this, -targetForce * releaseForce);


        ResetRope();
    }

    private void ResetRope()
    {
        State = RopeState.Idle;

        ropeLength = 0f;
        segmentLength = 0f;
        points.Clear();

        durability.SetValue(durability.MaxValue);
        CurrentTension = 0;

        OnReset?.Invoke();

        holder = null;
        target = null;

        DisableRopeCamera();
    }

    public void Detach()
    {
        holder?.OnRopeRelease(this, Vector2.zero);
        target?.OnRopeReleased(this, Vector2.zero);

        ResetRope();
    }

    public void Pull(float power)
    {
        if (State != RopeState.Attached)
            return;

        float reelAmount = pullDistance * power;

        SetRopeLength(ropeLength - reelAmount);

        Vector2 dir = (holder.AttachPoint.position - target.AttachPoint.position).normalized;
        dir.y = 0f;
        OnPullDirection?.Invoke(dir);

        holder?.OnRopeReel(dir * CurrentTension);
        target?.OnRopePulled(-dir * CurrentTension * pullForce);
    }

    public void Loosen()
    {
        SetRopeLength(ropeLength + (loosenLength * Time.deltaTime));
    }

    private void SetRopeLength(float newLength)
    {
        ropeLength = Mathf.Max(0.01f, newLength);

        if (points.Count > 1)
            segmentLength = ropeLength / (points.Count - 1);
    }

    private void FixedUpdate()
    {
        switch (State)
        {
            case RopeState.Casting:
                SimulateCast();
                break;

            case RopeState.Attached:
                SimulateAttached();
                break;
        }
    }

    private void SimulateCast()
    {
        if (points.Count < 2)
            return;

        Simulate();
        ApplyConstraints(holder.AttachPoint.position, castTipPosition);
    }

    private void SimulateAttached()
    {
        Simulate();

        ApplyConstraints(holder.AttachPoint.position, target.AttachPoint.position);

        UpdateMidPoint();

        UpdateTension();

        ApplyRopeForces();

        UpdateDurability();

        Loosen();
    }

    private void Simulate()
    {
        for (int i = 1; i < points.Count - 1; i++)
        {
            RopePoint p = points[i];

            Vector2 velocity = p.Position - p.PreviousPosition;

            p.PreviousPosition = p.Position;
            p.Position += velocity;

            p.Position += Vector2.up * ropeGravity * Time.deltaTime * Time.deltaTime;
        }
    }

    private void ApplyConstraints(Vector2 startPos, Vector2 endPos)
    {
        if (points.Count < 2)
            return;

        for (int iter = 0; iter < constraintIterations; iter++)
        {
            points[0].Position = startPos;
            points[^1].Position = endPos;

            for (int i = 0; i < points.Count - 1; i++)
            {
                RopePoint p1 = points[i];
                RopePoint p2 = points[i + 1];

                Vector2 delta = p2.Position - p1.Position;
                float dist = delta.magnitude;

                if (dist <= 0.0001f)
                    continue;

                float error = dist - segmentLength;
                Vector2 correction = delta.normalized * error;

                if (i == 0)
                {
                    p2.Position -= correction;
                }
                else if (i == points.Count - 2)
                {
                    p1.Position += correction;
                }
                else
                {
                    p1.Position += correction * 0.5f;
                    p2.Position -= correction * 0.5f;
                }
            }
        }
    }

    private void ApplyRopeForces()
    {
        Vector2 force = GetTensionForce();

        if (force == Vector2.zero)
            return;

        CalculateDistributedForce(
            force,
            out Vector2 holderForce,
            out Vector2 targetForce);

        holder.ApplyRopeForce(holderForce);
        target.ApplyRopeForce(targetForce);
    }

    private void CalculateDistributedForce(
        Vector2 ropeForce,
        out Vector2 holderForce,
        out Vector2 targetForce)
    {
        float totalWeight =
            holder.RopeWeight +
            target.RopeWeight;

        float holderInfluence =
            target.RopeWeight / totalWeight;

        float targetInfluence =
            holder.RopeWeight / totalWeight;

        holderForce = ropeForce * holderInfluence;
        targetForce = -ropeForce * targetInfluence;
    }

    private Vector2 GetTensionForce()
    {
        if (target == null || holder == null)
            return Vector2.zero;

        if (CurrentTension <= 0f)
            return Vector2.zero;

        Vector2 dir =
            (target.AttachPoint.position -
             holder.AttachPoint.position).normalized;

        return dir * CurrentTension * tensionForceMultiplier;
    }
    private void UpdateDurability()
    {
        if (CurrentTension > 0f)
        {
            noTensionTimer = 0f;

            float multiplier =
                CurrentTension >= dangerTension
                ? dangerDurabilityMultiplier
                : 1f;

            durability.SubtractValue(
                CurrentTension *
                durabilityLossPerTension *
                multiplier *
                Time.deltaTime);

            if (durability.CurrentValue <= 0f)
            {
                Release();
                return;
            }
        }
        else
        {
            noTensionTimer += Time.deltaTime;

            if (noTensionTimer >= recoverDelay)
            {
                durability.AddValue(
                    durabilityRecoverPerSecond *
                    Time.deltaTime);
            }
        }
    }

    private void UpdateTension()
    {
        if (target == null || holder == null)
        {
            CurrentTension = 0f;
            holder?.OnRopeTension(false);
            return;
        }

        float dist = Vector2.Distance(
            holder.AttachPoint.position,
            target.AttachPoint.position);

        CurrentTension = Mathf.Max(
            0f,
            dist - ropeLength - tensionThreshold);

        holder.OnRopeTension(CurrentTension > 0f);
    }

    private void UpdateMidPoint()
    {
        if (holder == null || target == null)
            return;

        Vector3 pos =
            (holder.AttachPoint.position + target.AttachPoint.position) * 0.5f;

        pos.y = 0f;

        midPoint.position = pos;
    }

    private void EnableRopeCamera()
    {
        ropeVirtualCamera.Priority = 20;
    }

    private void DisableRopeCamera()
    {
        ropeVirtualCamera.Priority = 0;
    }
}