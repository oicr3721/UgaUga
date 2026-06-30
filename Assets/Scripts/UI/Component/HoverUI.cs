using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private Ease ease = Ease.OutQuad;

    private Vector3 originalScale;
    private Tween scaleTween;

    private void Awake()
    {
        originalScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ScaleTo(originalScale * hoverScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ScaleTo(originalScale);
    }

    private void ScaleTo(Vector3 target)
    {
        scaleTween?.Kill();

        scaleTween = transform.DOScale(target, duration)
            .SetEase(ease);
    }

    private void OnDisable()
    {
        scaleTween?.Kill();
        transform.localScale = originalScale;
    }
}