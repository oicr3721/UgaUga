using System.Collections.Generic;
using UnityEngine;

public class HuntingStageManager : MonoBehaviour
{
    public static HuntingStageManager Instance { get; private set; }
    [SerializeField] private RewardTable rewardTable;

    private float timer = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        timer += Time.deltaTime;
    }

    public void OnAnimalCaptured(Animal animal)
    {
        // 게임 클리어 처리
        RewardCalculator.Calculate(rewardTable, timer, out ClearGrade grade, out int meatReward);

        StageResultData resultData = new StageResultData
        {
            Grade = grade,
            ClearTime = timer,
            MeatReward = meatReward,
            TeammateCount = 3
        };

        StageResultStorage.Save(resultData);

        StageEndingManager.Instance.PlaySuccess();
    }

    public void OnAnimalAtEndPoint()
    {
        Debug.Log("Failed !");

        StageResultData resultData = new StageResultData
        {
            Grade = ClearGrade.Fail,
            ClearTime = timer,
            MeatReward = 0,
            TeammateCount = 3
        };

        StageResultStorage.Save(resultData);

        StageEndingManager.Instance.PlayFail();
    }

}