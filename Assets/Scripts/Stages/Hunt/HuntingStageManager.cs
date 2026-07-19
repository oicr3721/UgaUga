using UnityEngine;

public class HuntingStageManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Animal animal;
    [SerializeField] private TeammateSpawner teammateSpawner;
    [SerializeField] private StageEndingManager stageEndingManager;

    [Header("Result")]
    [SerializeField] private RewardTable rewardTable;
    [Tooltip("현재 결과 데이터 형식을 보존하기 위한 팀원 수입니다.")]
    [Min(0)]
    [SerializeField] private int resultTeammateCount = 3;

    private float elapsedTime;

    private void OnEnable()
    {
        animal.Captured += HandleAnimalCaptured;
        animal.ReachedEndPoint += HandleAnimalReachedEndPoint;
        teammateSpawner.TeammateSpawned += HandleTeammateSpawned;
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
    }

    private void OnDisable()
    {
        animal.Captured -= HandleAnimalCaptured;
        animal.ReachedEndPoint -= HandleAnimalReachedEndPoint;
        teammateSpawner.TeammateSpawned -= HandleTeammateSpawned;
    }

    private void HandleAnimalCaptured(Animal capturedAnimal)
    {
        RewardCalculator.Calculate(
            rewardTable,
            elapsedTime,
            out ClearGrade grade,
            out int meatReward);

        SaveResult(grade, meatReward);
        stageEndingManager.PlaySuccess();
    }

    private void HandleAnimalReachedEndPoint()
    {
        SaveResult(ClearGrade.Fail, 0);
        stageEndingManager.PlayFail();
    }

    private void HandleTeammateSpawned(
        Teammate teammate,
        TeammateController teammateController)
    {
        stageEndingManager.AddTeamCharacter(teammate);
        stageEndingManager.AddDisableBehaviour(teammateController);
    }

    private void SaveResult(ClearGrade grade, int meatReward)
    {
        StageResultStorage.Save(new StageResultData
        {
            Grade = grade,
            ClearTime = elapsedTime,
            MeatReward = meatReward,
            TeammateCount = resultTeammateCount
        });
    }
}
