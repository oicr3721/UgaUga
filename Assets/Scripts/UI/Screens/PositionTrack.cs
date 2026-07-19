using System.Collections.Generic;
using UnityEngine;

public class PositionTrack : MonoBehaviour
{
    [Header("Marker Prefabs")]
    [SerializeField] private RectTransform teammateMarkerPrefab;
    [SerializeField] private RectTransform playerMarkerPrefab;
    [SerializeField] private RectTransform animalMarkerPrefab;

    [Header("World References")]
    [SerializeField] private TeammateSpawner teammateSpawner;
    [SerializeField] private Transform player;
    [SerializeField] private Transform animal;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Track UI")]
    [SerializeField] private RectTransform markerRoot;

    private readonly List<PositionMarker> positionMarkers = new();
    private float width;

    private readonly struct PositionMarker
    {
        public PositionMarker(Transform target, RectTransform marker)
        {
            Target = target;
            Marker = marker;
        }

        public Transform Target { get; }
        public RectTransform Marker { get; }
    }

    private void Awake()
    {
        width = markerRoot.rect.width;
        teammateSpawner.TeammateSpawned += HandleTeammateSpawned;
    }

    private void Start()
    {
        CreateMarker(playerMarkerPrefab, player);
        CreateMarker(animalMarkerPrefab, animal);
    }

    private void Update()
    {
        UpdateMarkerPositions();
    }

    private void OnDestroy()
    {
        teammateSpawner.TeammateSpawned -= HandleTeammateSpawned;
    }

    private void HandleTeammateSpawned(
        Teammate teammate,
        TeammateController teammateController)
    {
        CreateMarker(teammateMarkerPrefab, teammate.transform);
    }

    private void CreateMarker(RectTransform prefab, Transform target)
    {
        if (target == null)
            return;

        RectTransform marker = Instantiate(prefab, markerRoot);
        positionMarkers.Add(new PositionMarker(target, marker));
    }

    private void UpdateMarkerPositions()
    {
        foreach (PositionMarker positionMarker in positionMarkers)
        {
            float progress = Mathf.InverseLerp(
                startPoint.position.x,
                endPoint.position.x,
                positionMarker.Target.position.x);

            float markerPosition = Mathf.Lerp(
                -width * 0.5f,
                width * 0.5f,
                progress);

            Vector2 anchoredPosition = positionMarker.Marker.anchoredPosition;
            anchoredPosition.x = markerPosition;
            positionMarker.Marker.anchoredPosition = anchoredPosition;
        }
    }
}
