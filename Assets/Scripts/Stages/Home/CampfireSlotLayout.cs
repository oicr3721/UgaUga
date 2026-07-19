using System.Collections.Generic;
using UnityEngine;

public sealed class CampfireSlotLayout
{
    private readonly float firstSlotDistance;
    private readonly float spacing;
    private readonly float verticalOffset;

    public CampfireSlotLayout(float firstSlotDistance, float spacing, float verticalOffset)
    {
        this.firstSlotDistance = Mathf.Max(0.1f, firstSlotDistance);
        this.spacing = Mathf.Max(0.1f, spacing);
        this.verticalOffset = verticalOffset;
    }

    public IReadOnlyList<Vector2> CreateSlots(Vector2 center, int count)
    {
        List<Vector2> slots = new(count);

        for (int i = 0; i < count; i++)
        {
            int row = i / 2;
            float direction = i % 2 == 0 ? -1f : 1f;
            float horizontalDistance = firstSlotDistance + row * spacing;
            slots.Add(center + new Vector2(direction * horizontalDistance, verticalOffset));
        }

        return slots;
    }
}
