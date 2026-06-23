using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MouseActionControllerBase : MonoBehaviour
{
    public virtual void LeftStart() { }
    public virtual void LeftHold() { }
    public virtual void LeftStop() { }

    public virtual void RightStart() { }
    public virtual void RightHold() { }
    public virtual void RightStop() { }

    public virtual void ScrollUp() { }
    public virtual void ScrollDown() { }
}