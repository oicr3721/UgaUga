using UnityEngine;

public class RopePoint
{
    public Vector2 Position;
    public Vector2 PreviousPosition;

    public RopePoint(Vector2 position)
    {
        Position = position;
        PreviousPosition = position;
    }
}