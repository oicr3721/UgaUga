using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FloatingUI : AnimatedUI
{
    [Header("References")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation")]
    [SerializeField] private float moveOffset = 30f;
    [SerializeField] private float duration = 0.3f;
    [SerializeField] private Ease showEase = Ease.OutCubic;
    [SerializeField] private Ease hideEase = Ease.InCubic;

    private Vector2 originalPosition;

    protected override void Awake()
    {
        base.Awake();

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        originalPosition = rectTransform.anchoredPosition;
    }

    protected override void BeforeShow()
    {
        rectTransform.anchoredPosition = originalPosition - Vector2.up * moveOffset;
        canvasGroup.alpha = 0f;
    }

    protected override Tween CreateShowTween()
    {
        Sequence seq = DOTween.Sequence();

        seq.Join(
            rectTransform.DOAnchorPos(originalPosition, duration)
                .SetEase(showEase));

        seq.Join(
            canvasGroup.DOFade(1f, duration));

        return seq;
    }

    protected override Tween CreateHideTween()
    {
        Sequence seq = DOTween.Sequence();

        seq.Join(
            rectTransform.DOAnchorPos(originalPosition - Vector2.up * moveOffset, duration)
                .SetEase(hideEase));

        seq.Join(
            canvasGroup.DOFade(0f, duration));

        return seq;
    }

    protected override void AfterHide()
    {
        rectTransform.anchoredPosition = originalPosition;
        canvasGroup.alpha = 1f;
    }
}