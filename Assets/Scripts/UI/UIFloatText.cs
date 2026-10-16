using TMPro;
using UnityEngine;

/// <summary>Texto de UI que sobe e desaparece (ganho de tempo, dano, etc).</summary>
public class UIFloatText : MonoBehaviour
{
    private TextMeshProUGUI label;
    private RectTransform rect;
    private Color startColor;
    private float elapsed;
    private float duration = 0.9f;

    public void Begin(float seconds)
    {
        duration = Mathf.Max(0.05f, seconds);
        label = GetComponent<TextMeshProUGUI>();
        rect = GetComponent<RectTransform>();
        if (label != null)
        {
            startColor = label.color;
        }
    }

    private void Update()
    {
        if (label == null || rect == null)
        {
            Destroy(gameObject);
            return;
        }

        elapsed += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        rect.anchoredPosition += new Vector2(0f, 70f * Time.unscaledDeltaTime);
        label.color = startColor.WithAlpha(1f - (t * t));

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
