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
    [SerializeField] private Color dangerColor = Color.red;
    [SerializeField] private Color cautionColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    private Color currentColor;
    private float dangerTension;

    private void Awake()
    {
        rope.OnReset += ResetVisual;
        dangerTension = rope.DangerTension;
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
        Color targetColor;

        if (rope.CurrentTension <= 0f)
        {
            targetColor = normalColor;
        }
        else if (rope.CurrentTension < dangerTension)
        {
            float t =
                rope.CurrentTension / dangerTension;

            targetColor = Color.Lerp(
                normalColor,
                cautionColor,
                t);
        }
        else
        {
            float t =
                Mathf.Clamp01(
                    (rope.CurrentTension - dangerTension)
                    / dangerTension);

            targetColor = Color.Lerp(
                cautionColor,
                dangerColor,
                t);
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