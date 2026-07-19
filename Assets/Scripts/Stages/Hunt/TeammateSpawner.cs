using System;
using UnityEngine;

public class TeammateSpawner : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform attackTarget;

    [Header("Spawn Layout")]
    [Min(0f)]
    [SerializeField] private float spawnSpace;
    [SerializeField] private int spawnLayerOrder;

    public event Action<Teammate, TeammateController> TeammateSpawned;

    private void Start()
    {
        SpawnTeammates();
    }

    private void SpawnTeammates()
    {
        float spawnOffset = 0f;
        int sortingOrder = spawnLayerOrder;

        foreach (TeammateLoadout loadout in GameSession.TeammateLoadouts)
        {
            TeammateData teammateData = loadout.Teammate;
            Teammate prefab = teammateData != null ? teammateData.Prefab : null;

            if (prefab == null)
            {
                Debug.LogWarning("TeammateData에 생성할 프리팹이 지정되지 않았습니다.", teammateData);
                continue;
            }

            Teammate teammate = Instantiate(prefab, transform);
            teammate.Initialize(teammateData, loadout.EquippedWeapon);
            teammate.transform.position = transform.position + Vector3.left * spawnOffset;
            teammate.SpriteRenderer.sortingOrder = sortingOrder;

            TeammateController teammateController = teammate.GetComponentInChildren<TeammateController>();
            if (teammateController != null)
                teammateController.SetTarget(attackTarget);

            TeammateSpawned?.Invoke(teammate, teammateController);

            sortingOrder++;
            spawnOffset += spawnSpace;
        }
    }
}
