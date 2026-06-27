using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] renderers;

    [Header("Flash")]
    [SerializeField] private Color flashColor = new Color(1f, 0.45f, 0.45f, 1f);
    [SerializeField] private float flashTime = 0.06f;
    [SerializeField] private int flashCount = 2;

    private Coroutine flashRoutine;
    private Color[] originalColors;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>();

        originalColors = new Color[renderers.Length];
        CacheOriginalColors();
    }

    private void CacheOriginalColors()
    {
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;
    }

    public void Play()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        Restore();
        flashRoutine = StartCoroutine(CoPlay());
    }

    private IEnumerator CoPlay()
    {
        for (int i = 0; i < flashCount; i++)
        {
            SetColor(flashColor);
            yield return new WaitForSeconds(flashTime);

            Restore();
            if (i < flashCount - 1)
                yield return new WaitForSeconds(flashTime);
        }

        flashRoutine = null;
    }

    private void SetColor(Color color)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            renderers[i].color = color;
        }
    }

    private void Restore()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            renderers[i].color = originalColors[i];
        }
    }
}