using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeammateDataUI : MonoBehaviour
{
    [Header("Teammate")]
    [SerializeField] private TeammateCandidate targetTeammate;

    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private string meatEmojiName;
    [SerializeField] private TMP_Text meatCountTMP;
    [SerializeField] private TMP_Text attackTypeTMP;
    [SerializeField] private TMP_Text attackDamageTMP;
    [SerializeField] private TMP_Text speedTMP;

    [Header("Hover")]
    [SerializeField] private int hoverSortingOrder = 100;
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float hoverDuration = 0.15f;

    private int defaultSortingOrder;
    private Tween scaleTween;

    public void UpdateData(TeammateData data)
    {
        if (data == null) return;

        string meatText = "";
        for(int i = 0; i < data.meatCount; i++)
        {
            meatText += meatEmojiName;
        }
        meatCountTMP.text = meatText;

        //아래부터는 임시.
        //나중에 이모지로 바꿀 것.
        attackTypeTMP.text = "Type:" + data.type.ToString();
        attackDamageTMP.text = "Power:" + data.attackDamage.ToString();
        speedTMP.text = "Speed:" + data.speed.ToString();
    }

    public void HoverEnter()
    {
        canvas.sortingOrder = hoverSortingOrder;

        scaleTween?.Kill();
        scaleTween = transform.DOScale(
            hoverScale,
            hoverDuration);
    }

    public void HoverExit()
    {
        canvas.sortingOrder = defaultSortingOrder;

        scaleTween?.Kill();
        scaleTween = transform.DOScale(
            1f,
            hoverDuration);
    }
}
