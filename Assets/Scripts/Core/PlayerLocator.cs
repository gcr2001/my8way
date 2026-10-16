using UnityEngine;

/// <summary>
/// Cache da referencia do jogador. Evita que cada inimigo faca uma busca na
/// cena por frame. A referencia e invalidada automaticamente quando a cena
/// troca, porque objetos destruidos comparam como nulos.
/// </summary>
public static class PlayerLocator
{
    private static PlayerHealth cached;

    public static PlayerHealth Health
    {
        get
        {
            if (cached == null)
            {
                cached = Object.FindAnyObjectByType<PlayerHealth>();
            }

            return cached;
        }
    }

    public static PlayerMovement Movement
    {
        get
        {
            PlayerHealth health = Health;
            return health != null ? health.GetComponent<PlayerMovement>() : null;
        }
    }
}
