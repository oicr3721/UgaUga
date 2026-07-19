using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Runtime State")]
    [SerializeField] private ObservableValue playerMeat;
    [SerializeField] private List<TeammateData> teammateDatas = new();
    [SerializeField] private List<WeaponStack> initialWeapons = new();

    [Header("Team")]
    [Min(1)]
    [SerializeField] private int maxTeammateCount = 4;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        GameSession.Initialize(playerMeat, teammateDatas, initialWeapons, maxTeammateCount);
        DontDestroyOnLoad(this);
    }
}
