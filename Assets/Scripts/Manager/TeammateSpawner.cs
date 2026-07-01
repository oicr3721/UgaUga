using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeammateSpawner : MonoBehaviour
{
    [SerializeField] private Teammate meleeTeammate;
    [SerializeField] private Teammate rangedTeammate;
    [SerializeField] private Transform attackTarget;

    [SerializeField] private float spawnSpace; //스폰 간격
    [SerializeField] private int spawnLayerOrder = 0; //스프라이트 레이어 순서

    // Start is called before the first frame update
    void Awake()
    {
        SpawnTeammates();
    }

    void SpawnTeammates()
    {
        var teammateDatas = GameManager.Instance.TeammateDatas;

        float spawnX = 0;

        foreach (var teammateData in teammateDatas)
        {
            Teammate prefab = GetPrefab(teammateData.type);

            if (prefab == null)
            {
                Debug.LogWarning($"{teammateData.type}의 프리팹 없음");
                continue;
            }

            Teammate tm = Instantiate(prefab, transform);

            tm.Initialize(teammateData);
            tm.transform.position = transform.position + Vector3.left * spawnX;
            StageEndingManager.Instance.AddTeamCharacter(tm);

            TeammateController tmc = tm.GetComponentInChildren<TeammateController>();
            tmc.SetTarget(attackTarget);
            StageEndingManager.Instance.AddDisableBehaviour(tmc);

            tm.SpriteRenderer.sortingOrder = spawnLayerOrder;
            spawnLayerOrder++;

            spawnX += spawnSpace;
        }
    }
    private Teammate GetPrefab(TeammateType type)
    {
        return type switch
        {
            TeammateType.Melee => meleeTeammate,
            TeammateType.Ranged => rangedTeammate,
            _ => null
        };
    }
}
