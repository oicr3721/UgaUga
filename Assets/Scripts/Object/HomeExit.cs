using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeExit : MonoBehaviour, IInteractable
{
    public bool CanInteract => canInteract;

    public Transform InteractionPoint => transform;

    public string InteractionText => "사냥터로";

    private bool canInteract = true;

    public void Interact()
    {
        if(TeamManager.Instance.IsTeamValid)
        {
            canInteract = false;
            TeamManager.Instance.ConfirmTeam();

            SceneLoader.Load(SceneType.Hunt);
        }
        else
        {
            Debug.Log("유효하지 않은 팀 편성");
        }
    }

    public void OnHoverEnter()
    {
        
    }

    public void OnHoverExit()
    {
        
    }
}
