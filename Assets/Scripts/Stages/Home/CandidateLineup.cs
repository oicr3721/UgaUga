using System.Collections.Generic;
using UnityEngine;

public sealed class CandidateLineup
{
    private readonly Transform exitTransform;
    private readonly Transform cutlineTransform;
    private readonly float spacing;
    private readonly Vector2 waitingPositionRange;

    public CandidateLineup(
        Transform exitTransform,
        Transform cutlineTransform,
        float spacing,
        Vector2 waitingPositionRange)
    {
        this.exitTransform = exitTransform;
        this.cutlineTransform = cutlineTransform;
        this.spacing = spacing;
        this.waitingPositionRange = waitingPositionRange;
    }

    public void SetCutline(int maxTeammateCount)
    {
        cutlineTransform.position =
            exitTransform.position +
            Vector3.right * ((maxTeammateCount + 0.5f) * spacing);
    }

    public void Arrange(IReadOnlyList<TeammateCandidate> candidates)
    {
        for (int i = 0; i < candidates.Count; i++)
            candidates[i].SetDestination(GetLinePosition(i));
    }

    public void PlaceImmediately(TeammateCandidate candidate, int index)
    {
        candidate.transform.position = GetLinePosition(index);
    }

    public void MoveToWaitingPosition(TeammateCandidate candidate)
    {
        Vector2 waitingPosition = Vector2.right * Random.Range(
            waitingPositionRange.x,
            waitingPositionRange.y);

        candidate.SetDestination(waitingPosition);
    }

    private Vector2 GetLinePosition(int index)
    {
        return (Vector2)exitTransform.position +
            Vector2.right * ((index + 1) * spacing);
    }
}
