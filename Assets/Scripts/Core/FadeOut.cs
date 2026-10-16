using UnityEngine;

/// <summary>
/// Apaga um SpriteRenderer ao longo de alguns segundos e destroi o objeto.
/// Usado nos rastros de investida e nos efeitos de impacto.
/// </summary>
public class FadeOut : MonoBehaviour
{
    private SpriteRenderer target;
    private Color startColor;
    private float duration = 0.3f;
    private float elapsed;
    private float growth;

    public void Begin(float seconds, float scaleGrowth = 0f)
    {
        duration = Mathf.Max(0.01f, seconds);
        growth = scaleGrowth;
        elapsed = 0f;
        target = GetComponent<SpriteRenderer>();
        if (target != null)
        {
            startColor = target.color;
        }
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        target.color = startColor.WithAlpha(startColor.a * (1f - t));

        if (growth > 0f)
        {
            transform.localScale += Vector3.one * (growth * Time.unscaledDeltaTime);
        }

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
