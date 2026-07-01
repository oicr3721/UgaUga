using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactRange = 1f;
    [SerializeField] private LayerMask interactableMask;

    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionText;

    private IInteractable interactTarget;

    public void InteractInput(InputAction.CallbackContext context)
    {
        if (context.started)
            interactTarget?.Interact();
    }

    private void Update()
    {
        DetectInteractable();
        UpdateUI();
    }

    private void DetectInteractable()
    {
        IInteractable newTarget = null;
        float closestDist = float.MaxValue;

        var hits = Physics2D.OverlapCircleAll(
            transform.position,
            interactRange,
            interactableMask);

        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<IInteractable>();

            if (interactable == null || !interactable.CanInteract)
                continue;

            float dist = Vector2.Distance(
                transform.position,
                hit.transform.position);

            if (dist < closestDist)
            {
                closestDist = dist;
                newTarget = interactable;
            }
        }

        // 타겟이 그대로면 Hover 이벤트는 호출하지 않음
        if (newTarget == interactTarget)
            return;

        interactTarget?.OnHoverExit();

        interactTarget = newTarget;

        interactTarget?.OnHoverEnter();

        interactionPrompt.SetActive(interactTarget != null);
    }

    private void UpdateUI()
    {
        if (interactTarget == null || !interactionPrompt.activeSelf)
            return;

        // 텍스트는 런타임에 바뀔 수 있으므로 매 프레임 갱신
        interactionText.text = interactTarget.InteractionText;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(
            interactTarget.InteractionPoint.position);

        screenPos.z = 0;

        interactionPrompt.transform.position =
            screenPos + Vector3.down * 50f;
    }
}