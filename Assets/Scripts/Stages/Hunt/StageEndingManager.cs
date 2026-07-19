using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class StageEndingManager : MonoBehaviour
{
    [Header("Timelines")]
    [SerializeField] private PlayableDirector successTimeline;
    [SerializeField] private PlayableDirector failTimeline;

    [Header("Gameplay Objects")]
    [SerializeField] private List<Behaviour> disableBehaviours = new();
    [SerializeField] private List<Character> teamCharacters = new();

    public void PlaySuccess()
    {
        StartCoroutine(SuccessRoutine());
    }

    public void PlayFail()
    {
        StartCoroutine(FailRoutine());
    }

    public void AddDisableBehaviour(Behaviour behaviour)
    {
        disableBehaviours.Add(behaviour);
    }

    public void AddTeamCharacter(Character character)
    {
        teamCharacters.Add(character);
    }

    private IEnumerator SuccessRoutine()
    {
        DisableGameplay();
        successTimeline.Play();

        foreach (Character character in teamCharacters)
            character.Animator.SetTrigger("Dancing");

        yield return new WaitWhile(() => successTimeline.state == PlayState.Playing);
        SceneLoader.Load(SceneType.Result);
    }

    private IEnumerator FailRoutine()
    {
        DisableGameplay();
        failTimeline.Play();

        yield return new WaitWhile(() => failTimeline.state == PlayState.Playing);
        SceneLoader.Load(SceneType.Home);
    }

    private void DisableGameplay()
    {
        foreach (Behaviour behaviour in disableBehaviours)
            behaviour.enabled = false;
    }
}
