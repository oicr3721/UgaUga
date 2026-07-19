using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class Rope : MonoBehaviour
{
    private IRopeHoldable holder;
    private IRopeCatchable target;

    [Header("Verlet")]
    [Tooltip("로프를 구성하는 시뮬레이션 점 사이의 최소 간격입니다.")]
    [Min(0.001f)]
    [SerializeField] private float pointSpacing = 0.05f;
    [Min(0f)]
    [SerializeField] private float pullDistance = 1f;
    [Min(0f)]
    [SerializeField] private float tensionForceMultiplier = 30f;
    [Min(1)]
    [SerializeField] private int constraintIterations = 10;
    [SerializeField] private float ropeGravity = -1f;

    [Header("Durability")]
    [SerializeField] private ObservableValue durability;
    [Min(0f)]
    [SerializeField] private float durabilityLossPerTension = 1.5f;
    [Min(0f)]
    [SerializeField] private float durabilityRecoverPerSecond = 1f;
    [Min(0f)]
    [SerializeField] private float recoverDelay = 1.2f;
    [Min(0f)]
    [SerializeField] private float dangerDurabilityMultiplier = 2f;

    [Header("Rope")]
    [SerializeField] private Transform midPoint;
    [Min(0f)]
    [SerializeField] private float releaseForce = 0.5f;
    [Min(0f)]
    [SerializeField] private float pullForce = 1.2f;
    [Min(0f)]
    [SerializeField] private float loosenLength = 0.02f;

    [Header("Tension")]
    [Min(0f)]
    [SerializeField] private float tensionThreshold = 0.1f;
    [Min(0f)]
    [SerializeField] private float dangerTension = 2f;

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera ropeVirtualCamera;
    [SerializeField] private int activeCameraPriority = 20;
    [SerializeField] private int inactiveCameraPriority = 0;

    private readonly RopeSimulation simulation = new();
    private RopeCameraFeedback cameraFeedback;
    private Vector2 castTipPosition;
    private float noTensionTimer;

    public float CurrentTension { get; private set; }
    public float DangerTension => dangerTension;
    public float CurrentDurability => durability.CurrentValue;
    public float MaxDurability => durability.MaxValue;
    public IReadOnlyList<RopePoint> Points => simulation.Points;
    public RopeState State { get; private set; } = RopeState.Idle;

    public event Action<Vector2> OnPullDirection;
    public event Action RopeAttached;
    public event Action RopeReset;

    public enum RopeState
    {
        Idle,
        Casting,
        Attached
    }

    private void Awake()
    {
        durability.SetValue(durability.MaxValue);
        cameraFeedback = new RopeCameraFeedback(
            this,
            ropeVirtualCamera,
            activeCameraPriority,
            inactiveCameraPriority);
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

    private void OnDestroy()
    {
        cameraFeedback?.Dispose();
    }

    public void BeginCast(IRopeHoldable holdable, Vector2 startPosition)
    {
        Release();

        if (holdable == null)
            return;

        holder = holdable;
        target = null;
        simulation.Begin(startPosition);
        castTipPosition = startPosition;
        State = RopeState.Casting;
    }

    public void ExtendCast(Vector2 tipPosition)
    {
        if (State != RopeState.Casting || simulation.PointCount == 0)
            return;

        castTipPosition = tipPosition;
        simulation.Extend(tipPosition, pointSpacing);
    }

    public void CompleteCast(IRopeCatchable catchable)
    {
        if (State != RopeState.Casting || catchable == null || simulation.PointCount == 0)
            return;

        target = catchable;
        simulation.Complete(target.AttachPoint.position);

        holder?.OnRopeHold(this);
        target.OnRopeAttached(this);
        State = RopeState.Attached;
        RopeAttached?.Invoke();
    }

    public void Release()
    {
        if (target == null || holder == null)
        {
            ResetRope();
            return;
        }

        Vector2 force = GetTensionForce();
        CalculateDistributedForce(force, out Vector2 holderForce, out Vector2 targetForce);

        holder.OnRopeRelease(this, -holderForce * releaseForce);
        target.OnRopeReleased(this, -targetForce * releaseForce);
        ResetRope();
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
        simulation.SetLength(simulation.Length - reelAmount);

        Vector2 direction = (holder.AttachPoint.position - target.AttachPoint.position).normalized;
        direction.y = 0f;
        OnPullDirection?.Invoke(direction);

        holder?.OnRopeReel(direction * CurrentTension);
        target?.OnRopePulled(-direction * CurrentTension * pullForce);
    }

    public void Loosen()
    {
        simulation.SetLength(simulation.Length + (loosenLength * Time.deltaTime));
    }

    private void ResetRope()
    {
        State = RopeState.Idle;
        simulation.Clear();
        durability.SetValue(durability.MaxValue);
        CurrentTension = 0f;

        holder = null;
        target = null;

        RopeReset?.Invoke();
    }

    private void SimulateCast()
    {
        if (simulation.PointCount < 2)
            return;

        simulation.Simulate(ropeGravity, Time.deltaTime);
        simulation.ApplyConstraints(
            holder.AttachPoint.position,
            castTipPosition,
            constraintIterations);
    }

    private void SimulateAttached()
    {
        simulation.Simulate(ropeGravity, Time.deltaTime);
        simulation.ApplyConstraints(
            holder.AttachPoint.position,
            target.AttachPoint.position,
            constraintIterations);

        UpdateMidPoint();
        UpdateTension();
        ApplyRopeForces();
        UpdateDurability();
        Loosen();
    }

    private void ApplyRopeForces()
    {
        Vector2 force = GetTensionForce();

        if (force == Vector2.zero)
            return;

        CalculateDistributedForce(force, out Vector2 holderForce, out Vector2 targetForce);
        holder.ApplyRopeForce(holderForce);
        target.ApplyRopeForce(targetForce);
    }

    private void CalculateDistributedForce(
        Vector2 ropeForce,
        out Vector2 holderForce,
        out Vector2 targetForce)
    {
        float totalWeight = holder.RopeWeight + target.RopeWeight;
        float holderInfluence = target.RopeWeight / totalWeight;
        float targetInfluence = holder.RopeWeight / totalWeight;

        holderForce = ropeForce * holderInfluence;
        targetForce = -ropeForce * targetInfluence;
    }

    private Vector2 GetTensionForce()
    {
        if (target == null || holder == null || CurrentTension <= 0f)
            return Vector2.zero;

        Vector2 direction =
            (target.AttachPoint.position - holder.AttachPoint.position).normalized;

        return direction * CurrentTension * tensionForceMultiplier;
    }

    private void UpdateDurability()
    {
        if (CurrentTension > 0f)
        {
            noTensionTimer = 0f;

            float multiplier = CurrentTension >= dangerTension
                ? dangerDurabilityMultiplier
                : 1f;

            durability.SubtractValue(
                CurrentTension *
                durabilityLossPerTension *
                multiplier *
                Time.deltaTime);

            if (durability.CurrentValue <= 0f)
                Release();

            return;
        }

        noTensionTimer += Time.deltaTime;

        if (noTensionTimer >= recoverDelay)
            durability.AddValue(durabilityRecoverPerSecond * Time.deltaTime);
    }

    private void UpdateTension()
    {
        if (target == null || holder == null)
        {
            CurrentTension = 0f;
            holder?.OnRopeTension(false);
            return;
        }

        float distance = Vector2.Distance(
            holder.AttachPoint.position,
            target.AttachPoint.position);

        CurrentTension = Mathf.Max(
            0f,
            distance - simulation.Length - tensionThreshold);

        bool hasTension = CurrentTension > 0f;
        holder.OnRopeTension(hasTension);
    }

    private void UpdateMidPoint()
    {
        if (holder == null || target == null)
            return;

        Vector3 position =
            (holder.AttachPoint.position + target.AttachPoint.position) * 0.5f;
        position.y = 0f;
        midPoint.position = position;
    }
}
