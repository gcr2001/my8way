using UnityEngine;

/// <summary>
/// Fragmento de dados coletavel. Alem de pontos, devolve segundos ao
/// cronometro - e o que mantem a partida viva.
/// </summary>
public class Shard : MonoBehaviour
{
    private Vector3 anchor;
    private float phase;
    private bool collected;

    private void Awake()
    {
        anchor = transform.position;
        phase = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        if (collected)
        {
            return;
        }

        float t = (Time.time * 2.4f) + phase;
        transform.position = anchor + new Vector3(0f, Mathf.Sin(t) * 0.09f, 0f);
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.6f) * 18f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;
        GameManager manager = GameManager.Instance;
        if (manager != null)
        {
            manager.CollectShard(transform.position);
        }

        SpawnBurst();
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    private void SpawnBurst()
    {
        SpriteRenderer burst = ArtFactory.NewSprite("ShardBurst", ArtFactory.Glow,
            Palette.Gold.WithAlpha(0.8f), null, transform.position, 25);
        burst.transform.localScale = Vector3.one * 0.7f;
        burst.gameObject.AddComponent<FadeOut>().Begin(0.3f, 3f);
    }
}
