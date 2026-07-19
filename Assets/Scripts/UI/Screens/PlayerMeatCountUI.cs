using DG.Tweening;
using UnityEngine;

public class PlayerMeatCountUI : CountTextUI
{
    [Header("Punch")]
    [SerializeField] private float punchScale = 0.25f;
    [SerializeField] private float duration = 0.25f;

    private RectTransform rect;

    protected override void Initialize()
    {
        source = GameSession.PlayerMeat;
        rect = transform as RectTransform;
    }

    protected override void Refresh(float current, float max)
    {
        base.Refresh(current, max);

        // 최초 표시(Start에서 Refresh 호출)는 애니메이션 안 함
        if (!Application.isPlaying)
            return;

        rect.DOKill();

        rect.localScale = Vector3.one;

        rect
            .DOPunchScale(
                Vector3.one * punchScale,
                duration,
                vibrato: 8,
                elasticity: 0.8f);
    }
}
