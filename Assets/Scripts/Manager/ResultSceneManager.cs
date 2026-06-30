using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

public class ResultSceneManager : MonoBehaviour
{
    [SerializeField] private ResultPanel resultPanel;
    [SerializeField] private Transform characters;

    [Header("Character Positions")]
    [SerializeField] private Vector2 enterPosition;
    [SerializeField] private Vector2 displayPosition;
    [SerializeField] private Vector2 exitPosition;

    [Header("Animation")]
    [SerializeField] private float moveDuration = 0.5f;
    [SerializeField] private Ease enterEase = Ease.OutCubic;
    [SerializeField] private Ease exitEase = Ease.InCubic;

    [Header("Teammate")]
    [SerializeField] private List<GameObject> teammates = new();

    private StageResultData resultData;

    private void Start()
    {
        SetVisibleTeammates();

        resultData = StageResultStorage.Current;
        StageResultStorage.Clear();

        resultPanel.OnConfirm += OnConfirmResult;

        // 화면 밖에서 시작
        characters.position = enterPosition;

        // 등장
        characters.DOMove(displayPosition, moveDuration)
            .SetEase(enterEase)
            .OnComplete(() =>
            {
                resultPanel.Show(resultData);
            });
    }

    private void OnConfirmResult()
    {
        // 퇴장
        characters.DOMove(exitPosition, moveDuration)
            .SetEase(exitEase)
            .OnComplete(() =>
            {
                SceneLoader.Load(SceneType.Home);
            });
    }

    private void SetVisibleTeammates()
    {
        foreach(var tm in teammates)
        {
            tm.SetActive(false);
        }

        int teammateCount = GameManager.Instance.TeammateDatas.Count;

        for (int i = 0; i < teammateCount; i++)
        {
            teammates[i].SetActive(true);
        }
    }

    private void OnDestroy()
    {
        resultPanel.OnConfirm -= OnConfirmResult;
    }
}