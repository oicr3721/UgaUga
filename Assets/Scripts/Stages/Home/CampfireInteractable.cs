using UnityEngine;

public class CampfireInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private CampfireCouncilController councilController;

    public bool CanInteract => councilController != null && councilController.CanStartCouncil;
    public Transform InteractionPoint => transform;
    public string InteractionText => "부족 회의";

    public void Interact()
    {
        councilController.BeginCouncil();
    }

    public void OnHoverEnter()
    {
    }

    public void OnHoverExit()
    {
    }
}
