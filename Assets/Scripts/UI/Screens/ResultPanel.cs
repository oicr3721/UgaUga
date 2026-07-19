using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ResultPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private AnimatedUI resultPanel;
    [SerializeField] private RectTransform canvasRoot;
    [SerializeField] private AnimatedButton button;

    [Header("Result")]
    [SerializeField] private TMP_Text gradeText;
    [SerializeField] private TMP_Text timeText;

    [SerializeField] private AnimatedUI gradeUI;
    [SerializeField] private AnimatedUI timeUI;

    [Header("Meat")]
    [SerializeField] private AnimatedUI meatPrefab;
    [SerializeField] private Transform meatContainer;
    [SerializeField] private RectTransform meatCountTarget;

    [Header("Show Animation")]
    [SerializeField] private float meatSpawnStartDelay = 0.5f;
    [SerializeField] private float meatSpawnDelay = 0.05f;

    [Header("Absorb Animation")]
    [SerializeField] private float absorbStartDelay = 0.3f;
    [SerializeField] private float absorbDelay = 0.04f;
    [SerializeField] private float absorbDuration = 0.35f;
    [SerializeField] private float absorbScale = 0.2f;

    private readonly List<AnimatedUI> meats = new();

    private Sequence showSequence;
    private bool isShowing;
    public event Action OnConfirm;
    public event Action<int> RewardCollected;

    private void Awake()
    {
        button.enabled = false;
    }

    public void Show(StageResultData result)
    {
        showSequence?.Kill();

        gradeText.text = result.Grade.ToString();
        timeText.text = TimeSpan
            .FromSeconds(result.ClearTime)
            .ToString(@"mm\:ss");

        ClearMeats();

        isShowing = true;

        showSequence = DOTween.Sequence();

        showSequence
            .Append(resultPanel.Show().OnComplete(() =>
            {
                button.enabled = true;
            }))
            .Append(timeUI.Show())
            .Append(CreateMeatSequence(result.MeatReward))
            .Append(gradeUI.Show())
            .OnComplete(() =>
            {
                isShowing = false;
            });
    }

    private Tween CreateMeatSequence(int meatCount)
    {
        Sequence seq = DOTween.Sequence();

        meats.Clear();

        for (int i = 0; i < meatCount; i++)
        {
            AnimatedUI meat = Instantiate(meatPrefab, meatContainer);

            meat.gameObject.SetActive(false);
            meat.transform.localScale = Vector3.one;

            meats.Add(meat);

            float startTime = meatSpawnStartDelay + i * meatSpawnDelay;
            seq.Insert(startTime, meat.Show());
        }

        return seq;
    }

    private Tween CreateAbsorbSequence()
    {
        Sequence seq = DOTween.Sequence();

        Vector3 targetPos = meatCountTarget.position;

        seq.AppendInterval(absorbStartDelay);

        foreach (var meat in meats)
        {
            meat.transform.SetParent(canvasRoot, true);
        }

        for (int i = 0; i < meats.Count; i++)
        {
            AnimatedUI meat = meats[i];
            RectTransform rect = meat.GetComponent<RectTransform>();

            Sequence absorb = DOTween.Sequence();


            absorb.Join(
                rect
                    .DOMove(targetPos, absorbDuration)
                    .SetEase(Ease.InQuad));

            absorb.Join(
                rect
                    .DOScale(absorbScale, absorbDuration)
                    .SetEase(Ease.InExpo));

            absorb.OnComplete(() =>
            {
                RewardCollected?.Invoke(1);
                Destroy(meat.gameObject);
            });

            seq.Insert(i * absorbDelay, absorb);
        }

        return seq;
    }

    private void ClearMeats()
    {
        foreach (Transform child in meatContainer)
            Destroy(child.gameObject);

        meats.Clear();
    }

    public void OnClick()
    {
        if (isShowing)
        {
            showSequence.Complete();
            return;
        }

        Hide();
    }

    private void Hide()
    {
        button.enabled = false;

        Sequence seq = DOTween.Sequence();

        seq.Append(CreateAbsorbSequence());

        seq.Append(resultPanel.Hide());

        seq.OnComplete(() =>
        {
            isShowing = false;

            OnConfirm?.Invoke();
        });
    }
}
