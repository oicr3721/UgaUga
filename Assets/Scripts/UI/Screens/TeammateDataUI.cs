using DG.Tweening;
using TMPro;
using UnityEngine;

public class TeammateDataUI : MonoBehaviour
{
    [Header("Teammate")]
    [SerializeField] private TeammateCandidate targetTeammate;

    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private string meatEmojiName;
    [SerializeField] private TMP_Text meatCountTMP;
    [SerializeField] private TMP_Text attackTypeTMP;
    [SerializeField] private TMP_Text attackDamageTMP;
    [SerializeField] private TMP_Text speedTMP;

    [Header("Hover")]
    [SerializeField] private int hoverSortingOrder = 100;
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float hoverDuration = 0.15f;

    private int defaultSortingOrder;
    private Tween scaleTween;

    private void Awake()
    {
        defaultSortingOrder = canvas.sortingOrder;
    }

    public void UpdateData(TeammateData data, WeaponData equippedWeapon = null)
    {
        if (data == null)
            return;

        string meatText = string.Empty;
        for (int i = 0; i < data.MeatCost; i++)
            meatText += meatEmojiName;

        meatCountTMP.text = meatText;

        WeaponData weapon = equippedWeapon != null ? equippedWeapon : data.DefaultWeapon;
        attackTypeTMP.text = "Type:" + (weapon != null ? weapon.DisplayName : "None");
        attackDamageTMP.text = "Power:" + (weapon != null ? weapon.BaseDamage : 0f);
        speedTMP.text = "Speed:" + data.MoveSpeed;
    }

    public void HoverEnter()
    {
        canvas.sortingOrder = hoverSortingOrder;
        scaleTween?.Kill();
        scaleTween = transform.DOScale(hoverScale, hoverDuration);
    }

    public void HoverExit()
    {
        canvas.sortingOrder = defaultSortingOrder;
        scaleTween?.Kill();
        scaleTween = transform.DOScale(1f, hoverDuration);
    }
}
