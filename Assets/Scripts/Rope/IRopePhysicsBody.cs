using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IRopePhysicsBody
{
    Transform AttachPoint { get; }
    float RopeWeight { get; }
    void ApplyRopeForce(Vector2 force);
}