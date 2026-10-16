using UnityEngine;

/// <summary>
/// Vida do jogador: aplica dano, janela de invulnerabilidade com pisca-pisca,
/// empurrao e reposicionamento no spawn. O dano em si (vidas e penalidade de
/// tempo) e contabilizado pelo <see cref="GameManager"/>.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    private const float InvulnerabilityTime = 1.6f;

    private SpriteRenderer sprite;
    private Rigidbody2D body;
    private PlayerMovement movement;
    private float invulnerableUntil;
    private float blinkTimer;

    public bool Invulnerable
    {
        get { return Time.time < invulnerableUntil; }
    }

    private void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        body = GetComponent<Rigidbody2D>();
        movement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (sprite == null)
        {
            return;
        }

        if (!Invulnerable)
        {
            sprite.enabled = true;
            return;
        }

        blinkTimer += Time.unscaledDeltaTime;
        if (blinkTimer >= 0.08f)
        {
            blinkTimer = 0f;
            sprite.enabled = !sprite.enabled;
        }
    }

    public void GrantInvulnerability(float seconds)
    {
        invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds);
    }

    /// <summary>Recebe dano vindo de uma direcao (inimigo ou perigo).</summary>
    public void TakeDamage(Vector2 sourcePosition)
    {
        GameManager manager = GameManager.Instance;
        if (Invulnerable || manager == null || manager.State != GameState.Running)
        {
            return;
        }

        GrantInvulnerability(InvulnerabilityTime);
        manager.DamagePlayer(transform.position);

        CameraShake shake = CameraShake.Instance;
        if (shake != null)
        {
            shake.Shake(0.35f, 0.35f);
        }

        SpawnImpact();

        if (manager.State != GameState.Running)
        {
            return;
        }

        // Empurra para longe da ameaca e devolve estamina para a fuga ser possivel.
        Vector2 away = ((Vector2)transform.position - sourcePosition).normalized;
        if (away.sqrMagnitude < 0.01f)
        {
            away = Vector2.up;
        }

        if (body != null)
        {
            body.linearVelocity = away * 9f;
        }

        if (movement != null)
        {
            movement.RefillStamina();
        }
    }

    private void SpawnImpact()
    {
        SpriteRenderer flash = ArtFactory.NewSprite("Impact", ArtFactory.Glow,
            Palette.Pink.WithAlpha(0.75f), null, transform.position, 30);
        flash.transform.localScale = Vector3.one * 1.2f;
        flash.gameObject.AddComponent<FadeOut>().Begin(0.35f, 2.5f);
    }
}
