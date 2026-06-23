using UnityEngine;

public class HuntingStageManager : MonoBehaviour
{
    public static
        HuntingStageManager Instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        Instance = this;
    }

    public void OnAnimalCaptured(
        Animal animal)
    {
        Debug.Log(
            $"Captured : {animal.name}");

        // 게임 클리어 처리
    }
}