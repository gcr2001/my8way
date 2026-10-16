using UnityEngine;

/// <summary>
/// No de energia: perigo estatico que gira no lugar. Nao bloqueia a passagem
/// (e um trigger), mas atravessa-lo custa uma vida e segundos do cronometro.
/// </summary>
public class HazardNode : MonoBehaviour
{
    public float SpinSpeed = 55f;

    private float phase;

    private void Awake()
    {
        phase = Random.value * Mathf.PI * 2f;
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, SpinSpeed * Time.deltaTime);

        float scale = 0.7f + (Mathf.Sin((Time.time * 3.2f) + phase) * 0.06f);
        transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Damage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        Damage(other);
    }

    private void Damage(Collider2D other)
    {
        if (other == null || !other.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(transform.position);
        }
    }
}
