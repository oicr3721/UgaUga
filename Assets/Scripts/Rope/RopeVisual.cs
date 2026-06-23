using UnityEngine;

public class RopeVisual : MonoBehaviour
{
    [Header("Component")]
    [SerializeField] private Rope rope;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Line Width")]
    [SerializeField] private float minWidth = 0.015f; 
    [SerializeField] private float maxWidth = 0.12f;

    [Header("Line Color")]
    [SerializeField] private float maxVisualTension = 5f;
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private Color cautionColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    private Color currentColor;

    private void Awake()
    {
        rope.OnReset += ResetVisual;
    }

    private void LateUpdate()
    {
        var points = rope.Points;

        lineRenderer.positionCount =
            points.Count;

        for (int i = 0;
            i < points.Count;
            i++)
        {
            lineRenderer.SetPosition(
                i,
                points[i].Position);
        }

        UpdateDurabilityWidth();
        UpdateTensionColor();
    }

    private void UpdateDurabilityWidth()
    {
        float durabilityRatio =
            rope.CurrentDurability /
            rope.MaxDurability;

        float width =
            Mathf.Lerp(
                minWidth,
                maxWidth,
                durabilityRatio);

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }

    private void UpdateTensionColor()
    {
        float tensionRatio =
            Mathf.Clamp01(
                rope.CurrentTension /
                maxVisualTension);

        Color targetColor;

        if (tensionRatio < 0.5f)
        {
            targetColor = Color.Lerp(
                normalColor,
                cautionColor,
                tensionRatio * 2f);
        }
        else
        {
            targetColor = Color.Lerp(
                cautionColor,
                dangerColor,
                (tensionRatio - 0.5f) * 2f);
        }

        currentColor = Color.Lerp(
            currentColor,
            targetColor,
            Time.deltaTime * 10f);

        lineRenderer.startColor = currentColor;
        lineRenderer.endColor = currentColor;
    }

    private void ResetVisual()
    {
        currentColor = Color.white;

        float width =
            Mathf.Lerp(
                minWidth,
                maxWidth,
                rope.CurrentDurability / rope.MaxDurability);

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }
}