using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Monta a arena de cada onda a partir das plantas de <see cref="ArenaLayouts"/>:
/// piso, paredes com colisao, fragmentos, inimigos e perigos. Tambem ajusta a
/// camera para a arena inteira sempre caber na tela (qualquer proporcao) e
/// remove os objetos do tutorial original que foram substituidos pela arena nova.
/// </summary>
public class ArenaBuilder : MonoBehaviour
{
    private const float CellSize = 1f;

    private static PhysicsMaterial2D slickMaterial;

    private Transform tileRoot;
    private Transform dynamicRoot;
    private PlayerMovement player;
    private Vector3 playerSpawn;
    private int gridWidth;
    private int gridHeight;

    public static ArenaBuilder Current { get; private set; }

    /// <summary>Area interna navegavel, usada para travar o jogador na arena.</summary>
    public static Rect PlayArea { get; private set; }

    /// <summary>Material sem atrito: evita o jogador "colar" nas paredes.</summary>
    public static PhysicsMaterial2D Slick
    {
        get
        {
            if (slickMaterial == null)
            {
                slickMaterial = new PhysicsMaterial2D("Slick");
                slickMaterial.friction = 0f;
                slickMaterial.bounciness = 0f;
            }

            return slickMaterial;
        }
    }

    public static ArenaBuilder Build()
    {
        if (Current != null)
        {
            return Current;
        }

        GameObject host = new GameObject("Arena");
        return host.AddComponent<ArenaBuilder>();
    }

    private void Awake()
    {
        Current = this;

        player = FindAnyObjectByType<PlayerMovement>();
        CaptureSpriteMaterial();
        RemoveTutorialPlaceholders();

        if (player == null)
        {
            player = CreatePlayer();
        }

        ConfigurePlayer();

        tileRoot = NewChild("Tiles");
        dynamicRoot = NewChild("Entities");

        // Descarta qualquer UI antiga da cena antes de montar o HUD proprio.
        UIFactory.DestroyExistingCanvases();

        Decor.Build(transform);
        GameManager.Ensure();
        HUDManager.Build();
        LoadWave(1);
    }

    private void OnDestroy()
    {
        if (Current == this)
        {
            Current = null;
        }
    }

    /// <summary>
    /// Reutiliza o material de sprite da cena (Sprite-Lit do URP 2D) para os
    /// objetos criados em tempo de execucao receberem a mesma iluminacao.
    /// </summary>
    private void CaptureSpriteMaterial()
    {
        if (player == null)
        {
            return;
        }

        SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            ArtFactory.SpriteMaterial = renderer.sharedMaterial;
        }
    }

    /// <summary>
    /// Apaga as paredes e os triangulos coletaveis colocados a mao no tutorial:
    /// a arena tematica e gerada por codigo e ocupa o mesmo espaco.
    /// </summary>
    private void RemoveTutorialPlaceholders()
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            GameObject go = roots[i];
            if (go == null || go == gameObject)
            {
                continue;
            }

            bool legacyWall = go.name.StartsWith("Wall");
            bool legacyShard = go.name.StartsWith("Triangle") || go.CompareTag("Coletavel");
            if (legacyWall || legacyShard)
            {
                go.SetActive(false);
                Destroy(go);
            }
        }
    }

    private PlayerMovement CreatePlayer()
    {
        GameObject go = new GameObject("Player");
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<Rigidbody2D>();
        go.AddComponent<CircleCollider2D>();
        go.AddComponent<AudioSource>();
        return go.AddComponent<PlayerMovement>();
    }

    private void ConfigurePlayer()
    {
        GameObject go = player.gameObject;
        go.tag = "Player";
        go.transform.localScale = Vector3.one * 0.72f;

        SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            renderer = go.AddComponent<SpriteRenderer>();
        }

        renderer.sprite = ArtFactory.Player;
        renderer.color = Color.white;
        renderer.sortingOrder = 20;
        if (ArtFactory.SpriteMaterial != null)
        {
            renderer.sharedMaterial = ArtFactory.SpriteMaterial;
        }

        Rigidbody2D body = go.GetComponent<Rigidbody2D>();
        if (body == null)
        {
            body = go.AddComponent<Rigidbody2D>();
        }

        body.gravityScale = 0f;
        body.bodyType = RigidbodyType2D.Dynamic;
        body.freezeRotation = true;
        body.linearDamping = 0f;
        body.mass = 1f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D circle = go.GetComponent<CircleCollider2D>();
        if (circle == null)
        {
            circle = go.AddComponent<CircleCollider2D>();
        }

        circle.radius = 0.5f;
        circle.isTrigger = false;
        circle.sharedMaterial = Slick;

        if (go.GetComponent<PlayerHealth>() == null)
        {
            go.AddComponent<PlayerHealth>();
        }
    }

    private Transform NewChild(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        return go.transform;
    }

    /// <summary>Agenda a proxima onda, dando tempo para o aviso aparecer.</summary>
    public void ScheduleWave(int wave, float delay)
    {
        StartCoroutine(LoadWaveRoutine(wave, delay));
    }

    private IEnumerator LoadWaveRoutine(int wave, float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadWave(wave);
    }

    public void LoadWave(int wave)
    {
        string[] layout = ArenaLayouts.ForWave(wave);
        gridHeight = layout.Length;
        gridWidth = layout[0].Length;

        float halfWidth = ((gridWidth - 2) * CellSize) * 0.5f;
        float halfHeight = ((gridHeight - 2) * CellSize) * 0.5f;
        PlayArea = new Rect(-halfWidth, -halfHeight, halfWidth * 2f, halfHeight * 2f);

        ClearChildren(tileRoot);
        ClearChildren(dynamicRoot);

        int shards = 0;
        playerSpawn = Vector3.zero;

        for (int row = 0; row < gridHeight; row++)
        {
            string line = layout[row];
            for (int col = 0; col < line.Length; col++)
            {
                char cell = line[col];
                Vector3 position = CellToWorld(col, row);

                if (cell == ArenaLayouts.Wall)
                {
                    CreateWall(position, col, row);
                    continue;
                }

                CreateFloor(position, col, row);

                switch (cell)
                {
                    case ArenaLayouts.PlayerSpawn:
                        playerSpawn = position;
                        break;
                    case ArenaLayouts.ShardSpawn:
                        CreateShard(position);
                        shards++;
                        break;
                    case ArenaLayouts.PatrolSpawn:
                        CreateSentinel(position, wave, PreferredAxis(layout, col, row));
                        break;
                    case ArenaLayouts.HunterSpawn:
                        CreateHunter(position, wave, PreferredAxis(layout, col, row));
                        break;
                    case ArenaLayouts.HazardSpawn:
                        CreateHazard(position, wave);
                        break;
                }
            }
        }

        SetupCamera();
        RespawnPlayer();

        GameManager manager = GameManager.Instance;
        if (manager != null)
        {
            manager.RegisterWave(wave, shards);
            manager.Announce("SETOR " + wave + " / " + GameManager.TotalWaves + " - " +
                             ArenaLayouts.NameForWave(wave), Palette.Cyan);
        }

        AudioManager.Instance.SpawnSound();
    }

    private Vector3 CellToWorld(int col, int row)
    {
        float x = (col - ((gridWidth - 1) * 0.5f)) * CellSize;
        float y = (((gridHeight - 1) * 0.5f) - row) * CellSize;
        return new Vector3(x, y, 0f);
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    private void CreateWall(Vector3 position, int col, int row)
    {
        // Variacao sutil de tom para a parede nao parecer uma textura repetida.
        float noise = Mathf.PerlinNoise(col * 0.45f, row * 0.45f);
        Color tint = Color.Lerp(Palette.White, Palette.Cyan, 0.12f).Shade(-0.18f + (noise * 0.2f));

        SpriteRenderer renderer = ArtFactory.NewSprite("Wall", ArtFactory.Wall, tint, tileRoot, position, 0);
        BoxCollider2D box = renderer.gameObject.AddComponent<BoxCollider2D>();
        box.size = new Vector2(CellSize, CellSize);
        box.sharedMaterial = Slick;
    }

    private void CreateFloor(Vector3 position, int col, int row)
    {
        float noise = Mathf.PerlinNoise(col * 0.3f, row * 0.3f);
        Color tint = Color.white.Shade(-0.1f + (noise * 0.18f));
        ArtFactory.NewSprite("Floor", ArtFactory.Floor, tint, tileRoot, position, -20);
    }

    private void CreateShard(Vector3 position)
    {
        SpriteRenderer renderer = ArtFactory.NewSprite("Shard", ArtFactory.Shard, Color.white,
            dynamicRoot, position, 10);
        renderer.transform.localScale = Vector3.one * 0.6f;

        CircleCollider2D collider = renderer.gameObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.55f;
        collider.isTrigger = true;

        renderer.gameObject.AddComponent<Shard>();
        AddGlow(renderer.transform, Palette.Gold, 1.6f);
    }

    private void CreateSentinel(Vector3 position, int wave, Vector2 axis)
    {
        GameObject go = CreateEnemyBody("Sentinel", ArtFactory.Sentinel, position, Palette.Pink);
        PatrolEnemy patrol = go.AddComponent<PatrolEnemy>();
        patrol.Speed = 2.1f + (0.45f * (wave - 1));
        patrol.Direction = axis;
    }

    private void CreateHunter(Vector3 position, int wave, Vector2 axis)
    {
        GameObject go = CreateEnemyBody("Hunter", ArtFactory.Hunter, position, Palette.Purple);
        HunterEnemy hunter = go.AddComponent<HunterEnemy>();
        hunter.PatrolSpeed = 1.7f + (0.3f * (wave - 1));
        hunter.ChaseSpeed = 2.6f + (0.45f * (wave - 1));
        hunter.DetectRadius = 5f + (0.75f * (wave - 1));
        hunter.Direction = axis;
    }

    /// <summary>
    /// Eixo de patrulha do inimigo: o corredor mais longo a partir da celula de
    /// spawn. Evita que ele nasca virado para uma parede.
    /// </summary>
    private static Vector2 PreferredAxis(string[] layout, int col, int row)
    {
        int horizontal = OpenRun(layout, col, row, 1, 0) + OpenRun(layout, col, row, -1, 0);
        int vertical = OpenRun(layout, col, row, 0, 1) + OpenRun(layout, col, row, 0, -1);

        Vector2 axis = horizontal >= vertical ? Vector2.right : Vector2.up;
        return Random.value < 0.5f ? -axis : axis;
    }

    private static int OpenRun(string[] layout, int col, int row, int stepX, int stepY)
    {
        int steps = 0;
        int x = col + stepX;
        int y = row + stepY;

        while (y >= 0 && y < layout.Length && x >= 0 && x < layout[y].Length &&
               layout[y][x] != ArenaLayouts.Wall)
        {
            steps++;
            x += stepX;
            y += stepY;
        }

        return steps;
    }

    private GameObject CreateEnemyBody(string name, Sprite sprite, Vector3 position, Color glow)
    {
        SpriteRenderer renderer = ArtFactory.NewSprite(name, sprite, Color.white, dynamicRoot, position, 15);
        GameObject go = renderer.gameObject;
        go.transform.localScale = Vector3.one * 0.72f;

        Rigidbody2D body = go.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.mass = 40f;
        body.linearDamping = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CircleCollider2D collider = go.AddComponent<CircleCollider2D>();
        collider.radius = 0.5f;
        collider.sharedMaterial = Slick;

        AddGlow(go.transform, glow, 1.8f);
        return go;
    }

    private void CreateHazard(Vector3 position, int wave)
    {
        SpriteRenderer renderer = ArtFactory.NewSprite("Hazard", ArtFactory.Hazard, Color.white,
            dynamicRoot, position, 8);
        renderer.transform.localScale = Vector3.one * 0.7f;

        CircleCollider2D collider = renderer.gameObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.34f;
        collider.isTrigger = true;

        HazardNode hazard = renderer.gameObject.AddComponent<HazardNode>();
        hazard.SpinSpeed = 55f + (18f * (wave - 1));

        AddGlow(renderer.transform, Palette.Orange, 1.7f);
    }

    private void AddGlow(Transform parent, Color color, float scale)
    {
        SpriteRenderer glow = ArtFactory.NewSprite("Glow", ArtFactory.Glow,
            color.WithAlpha(0.28f), parent, parent.position, -2);
        glow.transform.localPosition = Vector3.zero;
        glow.transform.localScale = Vector3.one * scale;
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }

        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Palette.Void;
        cam.transform.position = new Vector3(0f, 0f, -10f);

        float aspect = cam.aspect > 0.1f ? cam.aspect : 16f / 9f;
        float halfHeight = (gridHeight * 0.5f) + 1.1f;
        float halfWidth = (gridWidth * 0.5f) + 0.4f;
        cam.orthographicSize = Mathf.Max(halfHeight, halfWidth / aspect);

        if (cam.GetComponent<CameraShake>() == null)
        {
            cam.gameObject.AddComponent<CameraShake>();
        }
    }

    private void RespawnPlayer()
    {
        if (player == null)
        {
            return;
        }

        player.Teleport(playerSpawn);
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.GrantInvulnerability(1.5f);
        }
    }

    /// <summary>Posicao de spawn da onda atual (usada ao perder uma vida).</summary>
    public Vector3 PlayerSpawn
    {
        get { return playerSpawn; }
    }
}
