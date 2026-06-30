using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public ObservableValue PlayerMeat => playerMeat;
    [SerializeField] private ObservableValue playerMeat;

    public IReadOnlyList<TeammateData> TeammateDatas => teammateDatas;
    [SerializeField] private List<TeammateData> teammateDatas = new();

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);
    }
}
