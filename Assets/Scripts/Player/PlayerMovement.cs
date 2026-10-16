using UnityEngine;

/// <summary>
/// Movimentacao em 8 direcoes da esfera de dados, com corrida (sprint) e
/// investida (dash) consumindo estamina. Le o asset de Project-wide Actions,
/// entao funciona igual no teclado e no controle de Xbox/joystick.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5.4f;
    [SerializeField] private float sprintMultiplier = 1.55f;
    [SerializeField] private float acceleration = 60f;

    [Header("Investida")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.16f;
    [SerializeField] private float dashCost = 34f;
    [SerializeField] private float dashCooldown = 0.45f;

    [Header("Estamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegen = 26f;
    [SerializeField] private float sprintDrain = 22f;

    private Rigidbody2D body;
    private SpriteRenderer sprite;
    private Vector2 facing = Vector2.right;
    private float stamina;
    private float dashTimer;
    private float dashCooldownTimer;
    private float regenDelay;
    private float ghostTimer;
    private Vector2 dashDirection;

    /// <summary>Estamina normalizada (0..1) para a barra do HUD.</summary>
    public float Stamina01
    {
        get { return maxStamina <= 0f ? 0f : Mathf.Clamp01(stamina / maxStamina); }
    }

    public bool Dashing
    {
        get { return dashTimer > 0f; }
    }

    public bool DashReady
    {
        get { return dashCooldownTimer <= 0f && stamina >= dashCost; }
    }

    public bool Sprinting { get; private set; }

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        stamina = maxStamina;
    }

    private void Update()
    {
        if (!Playable())
        {
            return;
        }

        if (InputHub.DashPressed && DashReady && dashTimer <= 0f)
        {
            StartDash();
        }
    }

    private void FixedUpdate()
    {
        if (!Playable())
        {
            body.linearVelocity = Vector2.zero;
            return;
        }

        float delta = Time.fixedDeltaTime;
        Vector2 input = InputHub.MoveValue;
        if (input.sqrMagnitude > 0.01f)
        {
            facing = input.normalized;
        }

        if (dashTimer > 0f)
        {
            dashTimer -= delta;
            body.linearVelocity = dashDirection * dashSpeed;
            SpawnGhost(delta);
        }
        else
        {
            if (dashCooldownTimer > 0f)
            {
                dashCooldownTimer -= delta;
            }

            Sprinting = InputHub.SprintHeld && input.sqrMagnitude > 0.01f && stamina > 1f;
            float speed = moveSpeed * (Sprinting ? sprintMultiplier : 1f);
            Vector2 target = input * speed;
            body.linearVelocity = Vector2.MoveTowards(body.linearVelocity, target, acceleration * delta);

            if (Sprinting)
            {
                stamina = Mathf.Max(0f, stamina - (sprintDrain * delta));
                regenDelay = 0.2f;
            }
        }

        RecoverStamina(delta);
        ApplySquash(delta);
        ClampToArena();
    }

    private bool Playable()
    {
        GameManager manager = GameManager.Instance;
        return manager == null || manager.State == GameState.Running;
    }

    private void StartDash()
    {
        dashDirection = facing.sqrMagnitude > 0.01f ? facing.normalized : Vector2.right;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown + dashDuration;
        stamina = Mathf.Max(0f, stamina - dashCost);
        regenDelay = 0.35f;
        ghostTimer = 0f;
        AudioManager.Instance.DashSound();
    }

    private void RecoverStamina(float delta)
    {
        if (regenDelay > 0f)
        {
            regenDelay -= delta;
            return;
        }

        if (!Sprinting && dashTimer <= 0f)
        {
            stamina = Mathf.Min(maxStamina, stamina + (staminaRegen * delta));
        }
    }

    /// <summary>Deforma levemente o sprite conforme a velocidade (feedback).</summary>
    private void ApplySquash(float delta)
    {
        float speedRatio = Mathf.Clamp01(body.linearVelocity.magnitude / (moveSpeed * sprintMultiplier));
        float stretch = 1f + (speedRatio * (Dashing ? 0.3f : 0.12f));
        float squash = 1f / stretch;

        Vector3 scale = new Vector3(0.72f * stretch, 0.72f * squash, 1f);
        transform.localScale = Vector3.Lerp(transform.localScale, scale, delta * 12f);

        if (body.linearVelocity.sqrMagnitude > 0.05f)
        {
            float angle = Mathf.Atan2(body.linearVelocity.y, body.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.Euler(0f, 0f, angle), delta * 10f);
        }
    }

    /// <summary>Trava o jogador dentro da arena (seguranca anti-softlock).</summary>
    private void ClampToArena()
    {
        Rect area = ArenaBuilder.PlayArea;
        if (area.width <= 0f)
        {
            return;
        }

        const float margin = 0.38f;
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, area.xMin + margin, area.xMax - margin);
        position.y = Mathf.Clamp(position.y, area.yMin + margin, area.yMax - margin);
        transform.position = position;
    }

    private void SpawnGhost(float delta)
    {
        if (sprite == null)
        {
            return;
        }

        ghostTimer -= delta;
        if (ghostTimer > 0f)
        {
            return;
        }

        ghostTimer = 0.035f;
        SpriteRenderer ghost = ArtFactory.NewSprite("DashGhost", sprite.sprite,
            Palette.Cyan.WithAlpha(0.45f), null, transform.position, sprite.sortingOrder - 1);
        ghost.transform.localScale = transform.localScale;
        ghost.transform.rotation = transform.rotation;
        ghost.gameObject.AddComponent<FadeOut>().Begin(0.25f);
    }

    /// <summary>Reposiciona o jogador (inicio de onda ou apos perder vida).</summary>
    public void Teleport(Vector3 position)
    {
        transform.position = position;
        transform.rotation = Quaternion.identity;
        dashTimer = 0f;
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }
    }

    public void RefillStamina()
    {
        stamina = maxStamina;
        regenDelay = 0f;
    }
}
