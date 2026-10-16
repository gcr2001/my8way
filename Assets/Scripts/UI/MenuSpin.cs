using UnityEngine;

/// <summary>Giro e flutuacao suaves do mascote do menu inicial.</summary>
public class MenuSpin : MonoBehaviour
{
    private Vector3 anchor;

    private void Awake()
    {
        anchor = transform.position;
    }

    private void Update()
    {
        transform.Rotate(0f, 0f, 24f * Time.unscaledDeltaTime);
        float bob = Mathf.Sin(Time.unscaledTime * 1.4f) * 0.18f;
        transform.position = anchor + new Vector3(0f, bob, 0f);
    }
}
