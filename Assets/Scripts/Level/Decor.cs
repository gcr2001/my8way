using UnityEngine;

/// <summary>
/// Luzes ambientes que flutuam devagar pela arena. Puramente estetico: da
/// profundidade ao tema neon sem interferir na jogabilidade.
/// </summary>
public class Decor : MonoBehaviour
{
    private static readonly Color[] Tints =
    {
        new Color(0.24f, 0.95f, 1f),
        new Color(0.22f, 1f, 0.53f),
        new Color(0.75f, 0.3f, 1f),
    };

    private Vector3 drift;
    private float wobblePhase;
    private float wobbleAmount;

    public static void Build(Transform parent)
    {
        GameObject host = new GameObject("Decor");
        host.transform.SetParent(parent, false);

        for (int i = 0; i < 9; i++)
        {
            Color tint = Tints[i % Tints.Length];
            Vector3 position = new Vector3(Random.Range(-9f, 9f), Random.Range(-4.5f, 4.5f), 0f);

            SpriteRenderer renderer = ArtFactory.NewSprite("AmbientGlow", ArtFactory.Glow,
                tint.WithAlpha(Random.Range(0.05f, 0.11f)), host.transform, position, -15);
            renderer.transform.localScale = Vector3.one * Random.Range(2.5f, 5.5f);

            Decor decor = renderer.gameObject.AddComponent<Decor>();
            decor.drift = new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(-0.18f, 0.18f), 0f);
            decor.wobblePhase = Random.value * Mathf.PI * 2f;
            decor.wobbleAmount = Random.Range(0.15f, 0.4f);
        }
    }

    private void Update()
    {
        Vector3 position = transform.position + (drift * Time.deltaTime);

        // Mantem as luzes dentro da arena, invertendo a deriva nas bordas.
        if (Mathf.Abs(position.x) > 10f)
        {
            drift.x = -drift.x;
        }

        if (Mathf.Abs(position.y) > 5.5f)
        {
            drift.y = -drift.y;
        }

        float wobble = Mathf.Sin((Time.time * 0.8f) + wobblePhase) * wobbleAmount * Time.deltaTime;
        transform.position = position + new Vector3(0f, wobble, 0f);
    }
}
