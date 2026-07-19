using UnityEngine;

public class HomeExit : MonoBehaviour, IInteractable
{
    [SerializeField] private TeamManager teamManager;

    private bool canInteract = true;

    public bool CanInteract => canInteract;
    public Transform InteractionPoint => transform;
    public string InteractionText => "사냥터로";

    public void Interact()
    {
        if (!teamManager.IsTeamValid)
        {
            Debug.Log("유효하지 않은 팀 편성");
            return;
        }

        canInteract = false;
        teamManager.ConfirmTeam();
        SceneLoader.Load(SceneType.Hunt);
    }

    public void OnHoverEnter()
    {
    }

    public void OnHoverExit()
    {
    }
}
