using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    bool CanInteract {  get; }
    void Interact();

    void OnHoverEnter();
    void OnHoverExit();

    Transform InteractionPoint { get; }
    string InteractionText { get; }
}
