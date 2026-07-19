using UnityEngine;
using UnityEngine.UI;

public class TeamManagementPanelView : MonoBehaviour
{
    [SerializeField] private Image panelImage;
    [SerializeField] private Image totalShareFill;
    [SerializeField] private RectTransform inventoryDropZone;
    [SerializeField] private Transform weaponContainer;

    public RectTransform InventoryDropZone => inventoryDropZone;
    public Transform WeaponContainer => weaponContainer;

    public void SetTotalShare(float share)
    {
        totalShareFill.fillAmount = Mathf.Clamp01(share);
    }

    public void SetColors(Color panelColor, Color shareColor)
    {
        panelImage.color = panelColor;
        totalShareFill.color = shareColor;
    }
}
