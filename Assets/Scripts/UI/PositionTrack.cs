using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PositionTrack : MonoBehaviour
{
    [Header("Marker Prefabs")]
    [SerializeField] private RectTransform teammateMarkerPrefab;
    [SerializeField] private RectTransform playerMarkerPrefab;
    [SerializeField] private RectTransform animalMarkerPrefab;

    [Header("World References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform animal;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Track UI")]
    [SerializeField] private RectTransform markerRoot;
    private float width;

    private struct PositionMarkerUI
    {
        public Transform target;
        public RectTransform markerUI;
    }

    private readonly List<PositionMarkerUI> positionMarkers = new();

    void Start()
    {
        width = markerRoot.rect.width;

        foreach (var teammate in FindObjectsByType<Teammate>(FindObjectsSortMode.None))
            CreateMarkerUI(teammateMarkerPrefab, teammate.transform);

        CreateMarkerUI(playerMarkerPrefab, player);
        CreateMarkerUI(animalMarkerPrefab, animal);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMarkerPos();
    }

    void CreateMarkerUI(RectTransform prefab, Transform target)
    {
        if (target == null)
            return;

        RectTransform positionMarker = Instantiate(prefab, markerRoot);

        PositionMarkerUI positionMarkerUI = new PositionMarkerUI
        {
            target = target,
            markerUI = positionMarker,
        };

        positionMarkers.Add(positionMarkerUI);
    }

    void UpdateMarkerPos()
    {
        foreach (var positionMarker in positionMarkers)
        {
            float progress = Mathf.InverseLerp(
                startPoint.position.x,
                endPoint.position.x,
                positionMarker.target.position.x);

            float markerPos = Mathf.Lerp(
                -width * 0.5f,
                 width * 0.5f,
                 progress);

            Vector2 pos = positionMarker.markerUI.anchoredPosition;
            pos.x = markerPos;
            positionMarker.markerUI.anchoredPosition = pos;

            Debug.Log(
                $"start={startPoint.position.x}, " +
                $"target={positionMarker.target.position.x}, " +
                $"end={endPoint.position.x}, " +
                $"progress={progress}");
        }
    }
}
