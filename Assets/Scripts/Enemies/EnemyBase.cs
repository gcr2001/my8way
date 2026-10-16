using UnityEngine;

/// <summary>
/// Comportamento comum dos inimigos: aplicar dano ao encostar no jogador,
/// detectar quando ficaram presos em uma quina e girar o sprite na direcao do
/// movimento. As subclasses implementam apenas a decisao de direcao.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyBase : MonoBehaviour
{
    private const float StuckCheckInterval = 0.35f;
    private const float StuckDistance = 0.06f;

    protected Rigidbody2D Body;
    protected SpriteRenderer Sprite;
    protected Transform GlowTransform;

    private Vector2 lastPosition;
    private float stuckTimer;

    /// <summary>Direcao atual de deslocamento.</summary>
    public Vector2 Direction { get; set; }

    protected bool Playable
    {
        get
        {
            GameManager manager = GameManager.Instance;
            return manager == null || manager.State == GameState.Running;
        }
    }

    protected Transform PlayerTransform
    {
        get
        {
            PlayerHealth health = PlayerLocator.Health;
            return health != null ? health.transform : null;
        }
    }

    protected virtual void Awake()
    {
        Body = GetComponent<Rigidbody2D>();
        Sprite = GetComponent<SpriteRenderer>();

        if (transform.childCount > 0)
        {
            Transform first = transform.GetChild(0);
            if (first.name == "Glow")
            {
                GlowTransform = first;
            }
        }

        if (Direction.sqrMagnitude < 0.01f)
        {
            Direction = Random.value < 0.5f ? Vector2.right : Vector2.up;
        }

        lastPosition = transform.position;
    }

    protected virtual void FixedUpdate()
    {
        if (!Playable)
        {
            Body.linearVelocity = Vector2.zero;
            return;
        }

        float delta = Time.fixedDeltaTime;
        Think(delta);
        TrackStuck(delta);
        FaceMovement(delta);
    }

    /// <summary>Decide a direcao e aplica a velocidade do inimigo.</summary>
    protected abstract void Think(float delta);

    /// <summary>Chamado quando o inimigo deixou de conseguir avancar.</summary>
    protected abstract void OnStuck();

    /// <summary>Chamado ao bater em algo que nao e o jogador (parede, inimigo).</summary>
    protected virtual void OnBumped(Collision2D collision)
    {
    }

    /// <summary>Checa se existe um obstaculo solido logo a frente.</summary>
    protected bool ObstacleAhead(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
        {
            return false;
        }

        Vector2 probe = (Vector2)transform.position + (direction.normalized * 0.62f);
        Collider2D hit = Physics2D.OverlapCircle(probe, 0.1f);
        return hit != null && !hit.isTrigger && hit.gameObject != gameObject &&
               !hit.CompareTag("Player");
    }

    private void TrackStuck(float delta)
    {
        stuckTimer += delta;
        if (stuckTimer < StuckCheckInterval)
        {
            return;
        }

        float moved = Vector2.Distance(transform.position, lastPosition);
        stuckTimer = 0f;
        lastPosition = transform.position;

        if (moved < StuckDistance)
        {
            OnStuck();
        }
    }

    private void FaceMovement(float delta)
    {
        if (Body.linearVelocity.sqrMagnitude < 0.05f)
        {
            return;
        }

        float angle = (Mathf.Atan2(Body.linearVelocity.y, Body.linearVelocity.x) * Mathf.Rad2Deg) - 90f;
        transform.rotation = Quaternion.Lerp(transform.rotation,
            Quaternion.Euler(0f, 0f, angle), delta * 8f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleContact(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleContact(collision);
    }

    private void HandleContact(Collision2D collision)
    {
        Collider2D other = collision.collider;
        if (other == null)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(transform.position);
            }

            return;
        }

        OnBumped(collision);
    }
}
