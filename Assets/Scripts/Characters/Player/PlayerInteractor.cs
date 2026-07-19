using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [Min(0f)]
    [SerializeField] private float interactRange = 1f;
    [SerializeField] private LayerMask interactableMask;

    [Header("Prompt")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private Vector2 promptScreenOffset = new(0f, -50f);

    private Camera mainCamera;
    private IInteractable interactTarget;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        DetectInteractable();
        UpdatePrompt();
    }

    public void InteractInput(InputAction.CallbackContext context)
    {
        if (context.started)
            interactTarget?.Interact();
    }

    private void DetectInteractable()
    {
        IInteractable newTarget = null;
        float closestDistance = float.MaxValue;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            interactRange,
            interactableMask);

        foreach (Collider2D hit in hits)
        {
            IInteractable interactable = hit.GetComponent<IInteractable>();

            if (interactable == null || !interactable.CanInteract)
                continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);

            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            newTarget = interactable;
        }

        if (newTarget == interactTarget)
            return;

        interactTarget?.OnHoverExit();
        interactTarget = newTarget;
        interactTarget?.OnHoverEnter();
        interactionPrompt.SetActive(interactTarget != null);
    }

    private void UpdatePrompt()
    {
        if (interactTarget == null || !interactionPrompt.activeSelf)
            return;

        interactionText.text = interactTarget.InteractionText;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(
            interactTarget.InteractionPoint.position);
        screenPosition.z = 0f;

        interactionPrompt.transform.position =
            screenPosition + (Vector3)promptScreenOffset;
    }
}
