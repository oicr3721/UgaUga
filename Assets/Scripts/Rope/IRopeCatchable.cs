using UnityEngine;

public interface IRopeCatchable : IRopePhysicsBody
{ 
    void OnRopeAttached(Rope rope);
    void OnRopePulled(Vector2 dir);
    void OnRopeReleased(Rope rope, Vector2 releaseImpulse);

}