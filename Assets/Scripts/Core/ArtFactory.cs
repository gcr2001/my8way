using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ponto unico de acesso aos sprites do jogo (Assets/Resources/Sprites, gerados
/// por Tools/generate_sprites.py). Se algum arquivo faltar, um sprite simples e
/// gerado em memoria para o jogo nunca quebrar.
/// </summary>
public static class ArtFactory
{
    private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
    private static Material spriteMaterial;

    public static Sprite Player { get { return Get("player"); } }
    public static Sprite Shard { get { return Get("shard"); } }
    public static Sprite Wall { get { return Get("wall"); } }
    public static Sprite Floor { get { return Get("floor"); } }
    public static Sprite Sentinel { get { return Get("sentinel"); } }
    public static Sprite Hunter { get { return Get("hunter"); } }
    public static Sprite Hazard { get { return Get("hazard"); } }
    public static Sprite Heart { get { return Get("heart"); } }
    public static Sprite Glow { get { return Get("glow"); } }
    public static Sprite Pixel { get { return Get("pixel"); } }

    /// <summary>
    /// Material usado pelos sprites da cena original (Sprite-Lit do URP 2D).
    /// Reutilizar o mesmo material mantem a iluminacao consistente.
    /// </summary>
    public static Material SpriteMaterial
    {
        get { return spriteMaterial; }
        set { spriteMaterial = value; }
    }

    public static Sprite Get(string name)
    {
        Sprite sprite;
        if (Cache.TryGetValue(name, out sprite) && sprite != null)
        {
            return sprite;
        }

        sprite = Resources.Load<Sprite>("Sprites/" + name);
        if (sprite == null)
        {
            sprite = BuildFallback(name);
        }

        Cache[name] = sprite;
        return sprite;
    }

    /// <summary>Cria um GameObject com SpriteRenderer configurado.</summary>
    public static SpriteRenderer NewSprite(string name, Sprite sprite, Color color, Transform parent,
        Vector3 position, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        if (spriteMaterial != null)
        {
            renderer.sharedMaterial = spriteMaterial;
        }

        return renderer;
    }

    private static Sprite BuildFallback(string name)
    {
        const int size = 32;
        bool square = name == "wall" || name == "floor" || name == "pixel";
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        float center = (size - 1) * 0.5f;
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool inside = square ||
                              new Vector2(x - center, y - center).magnitude <= center;
                pixels[(y * size) + x] = inside ? (Color32)Color.white : new Color32(0, 0, 0, 0);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        sprite.name = name + " (fallback)";
        Debug.LogWarning("ArtFactory: sprite 'Sprites/" + name +
                         "' nao encontrado em Resources, usando fallback. " +
                         "Rode 'python3 Tools/generate_sprites.py'.");
        return sprite;
    }
}
