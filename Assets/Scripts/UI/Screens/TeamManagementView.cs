using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TeamManagementView : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Canvas rootCanvas;

    [Header("UI Prefabs")]
    [SerializeField] private TeamManagementPanelView panelPrefab;
    [SerializeField] private WeaponInventoryItemView weaponSlotPrefab;
    [SerializeField] private Image weaponDragGhostPrefab;

    [Header("Warning")]
    [Min(1)]
    [SerializeField] private int warningFlashCount = 3;
    [Min(0.01f)]
    [SerializeField] private float warningFlashInterval = 0.12f;
    [SerializeField] private Color warningColor = new(0.9f, 0.1f, 0.08f, 1f);

    private readonly Color normalPanelColor = new(0.12f, 0.1f, 0.08f, 0.96f);
    private readonly Color normalShareColor = new(0.95f, 0.72f, 0.16f, 1f);

    private TeamManager teamManager;
    private TeamManagementPanelView panel;
    private Image dragGhost;
    private WeaponData draggedWeapon;
    private Coroutine warningRoutine;
    private bool isVisible;

    public bool IsDraggingWeapon => draggedWeapon != null;

    private void Awake()
    {
        panel = Instantiate(panelPrefab, rootCanvas.transform, false);
        panel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsDraggingWeapon || dragGhost == null)
            return;

        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;

        Vector2 mousePosition = mouse.position.ReadValue();
        dragGhost.rectTransform.position = mousePosition;

        if (mouse.leftButton.wasReleasedThisFrame)
            CompleteDrag(mousePosition);
    }

    private void OnDestroy()
    {
        Unsubscribe();

        if (IsDraggingWeapon)
            ReturnDraggedWeaponToInventory();
    }

    public void Bind(TeamManager manager)
    {
        if (teamManager == manager)
            return;

        Unsubscribe();
        teamManager = manager;

        if (teamManager == null)
            return;

        teamManager.TeamChanged += Refresh;
        teamManager.ShareLimitExceeded += FlashShareWarning;
        GameSession.PlayerWeapons.Changed += Refresh;

        foreach (TeammateCandidate candidate in teamManager.AllCandidates)
            candidate.SetManagementView(this);

        Refresh();
    }

    public void SetVisible(bool visible)
    {
        isVisible = visible;
        panel.gameObject.SetActive(visible);

        if (!visible && IsDraggingWeapon)
            ReturnDraggedWeaponToInventory();
    }

    public void Refresh()
    {
        if (teamManager == null)
            return;

        panel.SetTotalShare(teamManager.TotalShare);

        if (!IsDraggingWeapon)
            RebuildWeaponInventory();
    }

    public bool BeginInventoryWeaponDrag(WeaponData weapon, Vector2 screenPosition)
    {
        if (!isVisible || IsDraggingWeapon || weapon == null)
            return false;

        // Reserve first so the inventory Changed event does not rebuild and destroy
        // the slot that is currently receiving pointer drag callbacks.
        draggedWeapon = weapon;
        if (!GameSession.PlayerWeapons.TryRemove(weapon))
        {
            draggedWeapon = null;
            return false;
        }

        BeginDrag(weapon, screenPosition);
        return true;
    }

    public bool BeginCandidateWeaponDrag(TeammateCandidate candidate, Vector2 screenPosition)
    {
        if (!isVisible || IsDraggingWeapon || candidate == null)
            return false;

        if (!teamManager.TryTakeEquippedWeapon(candidate, out WeaponData weapon))
            return false;

        BeginDrag(weapon, screenPosition);
        return true;
    }

    public void UpdateDragPosition(Vector2 screenPosition)
    {
        if (dragGhost != null)
            dragGhost.rectTransform.position = screenPosition;
    }

    public void CompleteDrag(Vector2 screenPosition)
    {
        if (!IsDraggingWeapon)
            return;

        TeammateCandidate targetCandidate = FindCandidateAt(screenPosition);
        if (targetCandidate != null &&
            teamManager.EquipTransferredWeapon(targetCandidate, draggedWeapon))
        {
            ClearDrag();
            return;
        }

        if (RectTransformUtility.RectangleContainsScreenPoint(
                panel.InventoryDropZone,
                screenPosition,
                rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera))
        {
            ReturnDraggedWeaponToInventory();
            return;
        }

        // Weapons never remain in the world after an invalid drop.
        ReturnDraggedWeaponToInventory();
    }

    public void FlashShareWarning()
    {
        if (warningRoutine != null)
            StopCoroutine(warningRoutine);

        warningRoutine = StartCoroutine(CoFlashShareWarning());
    }

    private void BeginDrag(WeaponData weapon, Vector2 screenPosition)
    {
        draggedWeapon = weapon;
        dragGhost = Instantiate(weaponDragGhostPrefab, rootCanvas.transform, false);
        dragGhost.sprite = weapon.Icon;
        dragGhost.color = weapon.Icon != null ? Color.white : new Color(0.75f, 0.65f, 0.5f);
        dragGhost.rectTransform.position = screenPosition;
        dragGhost.transform.SetAsLastSibling();
        Refresh();
    }

    private TeammateCandidate FindCandidateAt(Vector2 screenPosition)
    {
        Camera worldCamera = Camera.main;
        if (worldCamera == null)
            return null;

        Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPosition);
        foreach (Collider2D hit in hits)
        {
            TeammateCandidate candidate = hit.GetComponentInParent<TeammateCandidate>();
            if (candidate != null && candidate.IsManagementEnabled)
                return candidate;
        }

        return null;
    }

    private void ReturnDraggedWeaponToInventory()
    {
        if (draggedWeapon != null)
            GameSession.PlayerWeapons.Add(draggedWeapon);

        ClearDrag();
    }

    private void ClearDrag()
    {
        draggedWeapon = null;
        if (dragGhost != null)
            Destroy(dragGhost.gameObject);

        dragGhost = null;

        if (panel != null)
            Refresh();
    }

    private void RebuildWeaponInventory()
    {
        for (int i = panel.WeaponContainer.childCount - 1; i >= 0; i--)
            Destroy(panel.WeaponContainer.GetChild(i).gameObject);

        foreach (WeaponStack stack in GameSession.PlayerWeapons.Stacks)
        {
            for (int i = 0; i < stack.Count; i++)
            {
                WeaponInventoryItemView item = Instantiate(weaponSlotPrefab, panel.WeaponContainer, false);
                item.Bind(this, stack.Weapon);
            }
        }
    }

    private IEnumerator CoFlashShareWarning()
    {
        for (int i = 0; i < warningFlashCount; i++)
        {
            panel.SetColors(warningColor, warningColor);
            yield return new WaitForSecondsRealtime(warningFlashInterval);

            panel.SetColors(normalPanelColor, normalShareColor);
            yield return new WaitForSecondsRealtime(warningFlashInterval);
        }

        warningRoutine = null;
    }

    private void Unsubscribe()
    {
        if (teamManager != null)
        {
            teamManager.TeamChanged -= Refresh;
            teamManager.ShareLimitExceeded -= FlashShareWarning;
        }

        GameSession.PlayerWeapons.Changed -= Refresh;
    }
}
