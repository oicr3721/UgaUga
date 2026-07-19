using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WeaponInventoryItemView : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [SerializeField] private Image icon;

    private TeamManagementView managementView;
    private bool ownsActiveDrag;

    public WeaponData Weapon { get; private set; }

    public void Bind(TeamManagementView view, WeaponData weapon)
    {
        managementView = view;
        Weapon = weapon;
        icon.sprite = Weapon.Icon;
        icon.color = Weapon.Icon != null ? Color.white : new Color(0.75f, 0.65f, 0.5f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ownsActiveDrag = managementView.BeginInventoryWeaponDrag(Weapon, eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ownsActiveDrag)
            managementView.UpdateDragPosition(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!ownsActiveDrag)
            return;

        ownsActiveDrag = false;
        managementView.CompleteDrag(eventData.position);
    }
}
