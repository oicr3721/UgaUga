using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using System.Collections.Generic;


public class StageEndingManager : MonoBehaviour
{
    public static StageEndingManager Instance { get; private set; }

    [Header("Success")]
    [SerializeField] private PlayableDirector successTimeline;

    [Header("Fail")]
    [SerializeField] private PlayableDirector failTimeline;

    [SerializeField] private List<Behaviour> disableBehaviours;
    [SerializeField] private List<Character> teamCharacters;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaySuccess()
    {
        StartCoroutine(SuccessRoutine());
    }

    public void PlayFail()
    {
        StartCoroutine(FailRoutine());
    }

    private IEnumerator SuccessRoutine()
    {
        DisableGameplay();

        successTimeline.Play();

        foreach (var c in teamCharacters)
        {
            c.Animator.SetTrigger("Dancing");
        }

        // Timeline 종료 대기
        yield return new WaitWhile(() =>
            successTimeline.state == PlayState.Playing);

        SceneLoader.Load(SceneType.Result);
    }

    private IEnumerator FailRoutine()
    {
        //DisableGameplay();

        failTimeline.Play();

        yield return new WaitWhile(() =>
            failTimeline.state == PlayState.Playing);

        SceneLoader.Load(SceneType.Home);
    }
    private void DisableGameplay()
    {
        foreach (var behaviour in disableBehaviours)
        {
            behaviour.enabled = false;
        }
    }

    public void AddDisableBehaviour(Behaviour behaviour)
    {
        disableBehaviours.Add(behaviour);
    }

    public void AddTeamCharacter(Character character)
    {
        teamCharacters.Add(character);
    }
}