using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Events;

public class AnimatedButton : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler
{
    [Header("Scale Settings")]
    [SerializeField] private float hoverScale = 1.02f;
    [SerializeField] private float pressedScale = 0.98f;
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private Ease ease = Ease.OutQuad;

    [Header("Click Feedback")]
    [SerializeField] private float punchScale = 0.08f;
    [SerializeField] private float punchDuration = 0.15f;

    [Header("Event")]
    public UnityEvent onClick;

    private Vector3 originalScale;
    private Tween tween;

    private bool isHovered;
    private bool isPressed;

    private void Awake()
    {
        originalScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;

        if (!isPressed)
            PlayScale(originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        if (!isPressed)
            PlayScale(originalScale);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        PlayScale(originalScale * pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;

        PlayScale(isHovered ? originalScale * hoverScale : originalScale);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 클릭 펀치 효과
        transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 10, 0.8f);

        onClick?.Invoke();
    }

    private void PlayScale(Vector3 target)
    {
        tween?.Kill();
        tween = transform.DOScale(target, duration).SetEase(ease);
    }

    private void OnDisable()
    {
        tween?.Kill();
        transform.localScale = originalScale;
        isHovered = false;
        isPressed = false;
    }
}