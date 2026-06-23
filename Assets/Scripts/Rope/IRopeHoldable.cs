using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IRopeHoldable : IRopePhysicsBody
{
    void OnRopeHold(Rope rope);
    void OnRopeReel(Vector2 dir);
    void OnRopeRelease(Rope rope, Vector2 releaseImpulse);
}
