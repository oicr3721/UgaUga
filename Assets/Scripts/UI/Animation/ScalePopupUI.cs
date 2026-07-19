using DG.Tweening;
using UnityEngine;

public class ScalePopupUI : AnimatedUI
{
    [SerializeField] RectTransform target;

    [SerializeField] private float showDuration = 0.3f;
    [SerializeField] private float hideDuration = 0.2f;

    protected override Tween CreateShowTween()
    {
        return target.DOScale(1f, showDuration)
            .SetEase(Ease.OutBack);
    }

    protected override Tween CreateHideTween()
    {
        return target.DOScale(0f, hideDuration)
            .SetEase(Ease.InBack);
    }

    protected override void BeforeShow()
    {
        target.localScale = Vector3.zero;
    }

}