using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeammateDataUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text shareLabel;
    [SerializeField] private Image shareFill;
    [SerializeField] private TMP_Text strengthLabel;
    [SerializeField] private Image strengthFill;
    [SerializeField] private TMP_Text speedLabel;
    [SerializeField] private Image speedFill;

    [Header("Scale")]
    [Min(0.1f)]
    [SerializeField] private float maximumStrength = 5f;
    [Min(0.1f)]
    [SerializeField] private float maximumSpeed = 5f;

    [Header("Hover")]
    [SerializeField] private int hoverSortingOrder = 100;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float hoverDuration = 0.12f;

    private int defaultSortingOrder;
    private Tween scaleTween;

    private void Awake()
    {
        defaultSortingOrder = canvas.sortingOrder;
        SetVisible(false);
    }

    private void OnDestroy()
    {
        scaleTween?.Kill();
    }

    public void UpdateData(TeammateData data, WeaponData equippedWeapon = null)
    {
        if (data == null)
            return;

        shareLabel.text = $"지분 {data.ShareRate:P0}";
        strengthLabel.text = "힘";
        speedLabel.text = "속도";
        shareFill.fillAmount = Mathf.Clamp01(data.ShareRate);
        strengthFill.fillAmount = Mathf.Clamp01(data.Strength / maximumStrength);
        speedFill.fillAmount = Mathf.Clamp01(data.MoveSpeed / maximumSpeed);
    }

    public void HoverEnter()
    {
        canvas.sortingOrder = hoverSortingOrder;
        scaleTween?.Kill();
        scaleTween = transform.DOScale(hoverScale, hoverDuration);
    }

    public void SetVisible(bool visible)
    {
        canvas.enabled = visible;
    }

    public void HoverExit()
    {
        canvas.sortingOrder = defaultSortingOrder;
        scaleTween?.Kill();
        scaleTween = transform.DOScale(1f, hoverDuration);
    }
}
