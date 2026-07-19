using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ResultSceneManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private ResultPanel resultPanel;
    [SerializeField] private Transform characters;

    [Header("Character Positions")]
    [SerializeField] private Vector2 enterPosition;
    [SerializeField] private Vector2 displayPosition;
    [SerializeField] private Vector2 exitPosition;

    [Header("Animation")]
    [Min(0f)]
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private Ease enterEase = Ease.OutCubic;
    [SerializeField] private Ease exitEase = Ease.InCubic;

    [Header("Teammates")]
    [SerializeField] private List<GameObject> teammates = new();

    private StageResultData resultData;

    private void Start()
    {
        UpdateVisibleTeammates();

        resultData = StageResultStorage.Current;
        StageResultStorage.Clear();

        resultPanel.OnConfirm += HandleResultConfirmed;
        resultPanel.RewardCollected += HandleRewardCollected;

        characters.position = enterPosition;
        characters
            .DOMove(displayPosition, moveDuration)
            .SetEase(enterEase)
            .OnComplete(() => resultPanel.Show(resultData));
    }

    private void OnDestroy()
    {
        resultPanel.OnConfirm -= HandleResultConfirmed;
        resultPanel.RewardCollected -= HandleRewardCollected;
    }

    private void HandleResultConfirmed()
    {
        characters
            .DOMove(exitPosition, moveDuration)
            .SetEase(exitEase)
            .OnComplete(() => SceneLoader.Load(SceneType.Home));
    }

    private void HandleRewardCollected(int amount)
    {
        GameSession.PlayerMeat.AddValue(amount);
    }

    private void UpdateVisibleTeammates()
    {
        foreach (GameObject teammate in teammates)
            teammate.SetActive(false);

        int teammateCount = GameSession.TeammateLoadouts.Count;

        for (int i = 0; i < teammateCount; i++)
            teammates[i].SetActive(true);
    }
}
