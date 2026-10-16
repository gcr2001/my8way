using UnityEngine;

/// <summary>
/// Cacador: patrulha enquanto nao ve o jogador e passa a perseguir quando ele
/// entra no raio de deteccao (comportamento variavel). Ao travar em uma quina,
/// desliza para o lado por um instante para se desprender.
/// </summary>
public class HunterEnemy : EnemyBase
{
    public float PatrolSpeed = 1.7f;
    public float ChaseSpeed = 2.6f;
    public float DetectRadius = 5f;

    private float sidestepTimer;
    private Vector2 sidestepDirection;
    private bool chasing;

    protected override void Think(float delta)
    {
        Transform target = PlayerTransform;
        float distance = target != null
            ? Vector2.Distance(transform.position, target.position)
            : float.MaxValue;

        bool wantsChase = target != null && distance <= DetectRadius;
        if (wantsChase != chasing)
        {
            chasing = wantsChase;
            UpdateGlow();
        }

        if (sidestepTimer > 0f)
        {
            sidestepTimer -= delta;
            Body.linearVelocity = sidestepDirection * PatrolSpeed;
            return;
        }

        if (chasing)
        {
            Vector2 toTarget = ((Vector2)target.position - (Vector2)transform.position).normalized;
            Direction = toTarget;
            Body.linearVelocity = toTarget * ChaseSpeed;
            return;
        }

        if (ObstacleAhead(Direction))
        {
            Direction = -Direction;
        }

        Body.linearVelocity = Direction.normalized * PatrolSpeed;
    }

    protected override void OnStuck()
    {
        // Escolhe uma perpendicular para contornar o canto.
        Vector2 perpendicular = new Vector2(-Direction.y, Direction.x).normalized;
        if (Random.value < 0.5f)
        {
            perpendicular = -perpendicular;
        }

        sidestepDirection = perpendicular;
        sidestepTimer = 0.45f;
    }

    protected override void OnBumped(Collision2D collision)
    {
        if (chasing || sidestepTimer > 0f)
        {
            return;
        }

        Vector2 toOther = (Vector2)collision.transform.position - (Vector2)transform.position;
        if (Vector2.Dot(toOther.normalized, Direction.normalized) > 0.25f)
        {
            Direction = -Direction;
        }
    }

    private void UpdateGlow()
    {
        if (Sprite != null)
        {
            Sprite.color = chasing ? Color.white : Color.white.Shade(-0.18f);
        }

        if (GlowTransform != null)
        {
            SpriteRenderer glow = GlowTransform.GetComponent<SpriteRenderer>();
            if (glow != null)
            {
                glow.color = chasing
                    ? Palette.Pink.WithAlpha(0.5f)
                    : Palette.Purple.WithAlpha(0.28f);
            }

            GlowTransform.localScale = Vector3.one * (chasing ? 2.2f : 1.8f);
        }
    }
}
