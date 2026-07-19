using System.Collections.Generic;
using UnityEngine;

public sealed class RopeSimulation
{
    private readonly List<RopePoint> points = new();

    public IReadOnlyList<RopePoint> Points => points;
    public int PointCount => points.Count;
    public float Length { get; private set; }

    private float segmentLength;

    public void Begin(Vector2 startPosition)
    {
        points.Clear();
        points.Add(new RopePoint(startPosition));
        Length = 0f;
        segmentLength = 0f;
    }

    public void Extend(Vector2 tipPosition, float pointSpacing)
    {
        if (points.Count == 0)
            return;

        float distance = Vector2.Distance(points[^1].Position, tipPosition);

        if (distance < pointSpacing)
            return;

        RopePoint previousPoint = points[^1];
        previousPoint.PreviousPosition = previousPoint.Position;

        points.Add(new RopePoint(tipPosition));
        Length += distance;
        UpdateSegmentLength();
    }

    public void Complete(Vector2 finalPosition)
    {
        if (points.Count == 0)
            return;

        RopePoint previousPoint = points[^1];
        previousPoint.PreviousPosition = previousPoint.Position;

        Length += Vector2.Distance(points[^1].Position, finalPosition);
        points.Add(new RopePoint(finalPosition));
        UpdateSegmentLength();
    }

    public void SetLength(float newLength)
    {
        Length = Mathf.Max(0.01f, newLength);
        UpdateSegmentLength();
    }

    public void Simulate(float gravity, float deltaTime)
    {
        for (int i = 1; i < points.Count - 1; i++)
        {
            RopePoint point = points[i];
            Vector2 velocity = point.Position - point.PreviousPosition;

            point.PreviousPosition = point.Position;
            point.Position += velocity;
            point.Position += Vector2.up * gravity * deltaTime * deltaTime;
        }
    }

    public void ApplyConstraints(
        Vector2 startPosition,
        Vector2 endPosition,
        int constraintIterations)
    {
        if (points.Count < 2)
            return;

        for (int iteration = 0; iteration < constraintIterations; iteration++)
        {
            points[0].Position = startPosition;
            points[^1].Position = endPosition;

            for (int i = 0; i < points.Count - 1; i++)
            {
                RopePoint firstPoint = points[i];
                RopePoint secondPoint = points[i + 1];
                Vector2 delta = secondPoint.Position - firstPoint.Position;
                float distance = delta.magnitude;

                if (distance <= 0.0001f)
                    continue;

                float error = distance - segmentLength;
                Vector2 correction = delta.normalized * error;

                if (i == 0)
                {
                    secondPoint.Position -= correction;
                }
                else if (i == points.Count - 2)
                {
                    firstPoint.Position += correction;
                }
                else
                {
                    firstPoint.Position += correction * 0.5f;
                    secondPoint.Position -= correction * 0.5f;
                }
            }
        }
    }

    public void Clear()
    {
        Length = 0f;
        segmentLength = 0f;
        points.Clear();
    }

    private void UpdateSegmentLength()
    {
        segmentLength = points.Count > 1
            ? Length / (points.Count - 1)
            : 0f;
    }
}
