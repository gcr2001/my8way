using UnityEngine;

/// <summary>
/// Sentinela: percorre um eixo fixo (horizontal ou vertical) definido pela
/// planta da arena e inverte o sentido ao encontrar parede. Nao persegue, mas
/// bloqueia rotas e causa dano ao encostar.
/// </summary>
public class PatrolEnemy : EnemyBase
{
    public float Speed = 2.1f;

    private float pulsePhase;

    protected override void Awake()
    {
        base.Awake();
        pulsePhase = Random.value * Mathf.PI * 2f;
    }

    protected override void Think(float delta)
    {
        if (ObstacleAhead(Direction))
        {
            Reverse();
        }

        Body.linearVelocity = Direction.normalized * Speed;
        Pulse();
    }

    protected override void OnStuck()
    {
        Reverse();
    }

    protected override void OnBumped(Collision2D collision)
    {
        // Inverte somente se o obstaculo estiver realmente no caminho.
        Vector2 toOther = (Vector2)collision.transform.position - (Vector2)transform.position;
        if (Vector2.Dot(toOther.normalized, Direction.normalized) > 0.25f)
        {
            Reverse();
        }
    }

    private void Reverse()
    {
        Direction = -Direction;
    }

    private void Pulse()
    {
        if (GlowTransform == null)
        {
            return;
        }

        float scale = 1.7f + (Mathf.Sin((Time.time * 4f) + pulsePhase) * 0.14f);
        GlowTransform.localScale = Vector3.one * scale;
    }
}
