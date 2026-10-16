using UnityEngine;

/// <summary>Tremor de camera curto usado no feedback de dano e de vitoria.</summary>
public class CameraShake : MonoBehaviour
{
    private Vector3 basePosition;
    private float duration;
    private float remaining;
    private float magnitude;

    public static CameraShake Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        basePosition = transform.position;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Shake(float seconds, float strength)
    {
        basePosition = new Vector3(0f, 0f, transform.position.z);
        duration = Mathf.Max(0.01f, seconds);
        remaining = duration;
        magnitude = strength;
    }

    private void LateUpdate()
    {
        if (remaining <= 0f)
        {
            return;
        }

        // Tempo nao escalado para o tremor funcionar mesmo com o jogo pausando.
        remaining -= Time.unscaledDeltaTime;
        float falloff = Mathf.Clamp01(remaining / duration);
        Vector2 offset = Random.insideUnitCircle * (magnitude * falloff);
        transform.position = basePosition + new Vector3(offset.x, offset.y, 0f);

        if (remaining <= 0f)
        {
            transform.position = basePosition;
        }
    }
}
