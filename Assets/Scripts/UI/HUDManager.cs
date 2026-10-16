using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Monta e atualiza toda a interface da partida: placar, cronometro, setor,
/// fragmentos restantes, vidas, estamina, avisos e os paineis de pausa, fim de
/// jogo e vitoria. Todos os paineis tem botoes de reiniciar e de voltar ao
/// menu, garantindo que nunca exista um dead-end.
/// </summary>
public class HUDManager : MonoBehaviour
{
    private static HUDManager instance;

    private Canvas canvas;
    private RectTransform root;

    private TextMeshProUGUI scoreText;
    private TextMeshProUGUI highScoreText;
    private TextMeshProUGUI timeText;
    private TextMeshProUGUI sectorText;
    private TextMeshProUGUI shardText;
    private TextMeshProUGUI announceText;
    private TextMeshProUGUI hintText;
    private TextMeshProUGUI dashText;
    private Image timeBar;
    private Image staminaBar;
    private readonly List<Image> hearts = new List<Image>();

    private RectTransform pausePanel;
    private RectTransform endPanel;
    private TextMeshProUGUI endTitle;
    private TextMeshProUGUI endStats;
    private Button endPrimaryButton;
    private Button pausePrimaryButton;

    private float announceTimer;
    private Color announceColor = Color.white;

    public static HUDManager Instance
    {
        get { return instance; }
    }

    public static HUDManager Build()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject host = new GameObject("HUD");
        instance = host.AddComponent<HUDManager>();
        return instance;
    }

    private void Awake()
    {
        instance = this;
        canvas = UIFactory.CreateCanvas("HUDCanvas", 100);
        canvas.transform.SetParent(transform, false);
        root = canvas.GetComponent<RectTransform>();

        BuildTopBar();
        BuildBottomBar();
        BuildAnnounce();
        BuildPausePanel();
        BuildEndPanel();
    }

    private void OnEnable()
    {
        GameManager manager = GameManager.Instance;
        if (manager != null)
        {
            manager.StateChanged += HandleStateChanged;
            manager.Announced += HandleAnnounce;
            manager.Popup += HandlePopup;
        }
    }

    private void OnDisable()
    {
        GameManager manager = GameManager.Instance;
        if (manager != null)
        {
            manager.StateChanged -= HandleStateChanged;
            manager.Announced -= HandleAnnounce;
            manager.Popup -= HandlePopup;
        }

        if (instance == this)
        {
            instance = null;
        }
    }

    // --------------------------------------------------------------- layout --

    private void BuildTopBar()
    {
        Image bar = UIFactory.Panel("TopBar", root, Palette.Panel.WithAlpha(0.82f));
        UIFactory.Anchor(bar.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -116f), new Vector2(0f, 0f));

        Image edge = UIFactory.Panel("TopEdge", bar.rectTransform, Palette.Cyan.WithAlpha(0.5f));
        UIFactory.Anchor(edge.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, -3f), new Vector2(0f, 0f));

        scoreText = UIFactory.Text("Score", bar.rectTransform, "PONTOS 0", 40f,
            Palette.White, TextAlignmentOptions.TopLeft);
        UIFactory.Anchor(scoreText.rectTransform, new Vector2(0f, 0f), new Vector2(0.33f, 1f),
            new Vector2(36f, 8f), new Vector2(0f, -12f));

        highScoreText = UIFactory.Text("HighScore", bar.rectTransform, "RECORDE 0", 26f,
            Palette.Muted, TextAlignmentOptions.BottomLeft);
        UIFactory.Anchor(highScoreText.rectTransform, new Vector2(0f, 0f), new Vector2(0.33f, 1f),
            new Vector2(36f, 14f), new Vector2(0f, -62f));

        timeText = UIFactory.Text("Time", bar.rectTransform, "00:45.0", 58f,
            Palette.Cyan, TextAlignmentOptions.Top);
        UIFactory.Anchor(timeText.rectTransform, new Vector2(0.33f, 0f), new Vector2(0.67f, 1f),
            new Vector2(0f, 20f), new Vector2(0f, -4f));

        timeBar = UIFactory.Bar("TimeBar", bar.rectTransform, new Vector2(420f, 14f),
            Palette.Cyan, Palette.Void.WithAlpha(0.85f));
        RectTransform timeBarRect = timeBar.rectTransform.parent as RectTransform;
        timeBarRect.anchorMin = new Vector2(0.5f, 0f);
        timeBarRect.anchorMax = new Vector2(0.5f, 0f);
        timeBarRect.anchoredPosition = new Vector2(0f, 16f);

        sectorText = UIFactory.Text("Sector", bar.rectTransform, "SETOR 1/3", 32f,
            Palette.Green, TextAlignmentOptions.TopRight);
        UIFactory.Anchor(sectorText.rectTransform, new Vector2(0.67f, 0f), new Vector2(1f, 1f),
            new Vector2(0f, 8f), new Vector2(-36f, -14f));

        shardText = UIFactory.Text("Shards", bar.rectTransform, "FRAGMENTOS 0/0", 30f,
            Palette.Gold, TextAlignmentOptions.BottomRight);
        UIFactory.Anchor(shardText.rectTransform, new Vector2(0.67f, 0f), new Vector2(1f, 1f),
            new Vector2(0f, 14f), new Vector2(-36f, -60f));
    }

    private void BuildBottomBar()
    {
        Image bar = UIFactory.Panel("BottomBar", root, Palette.Panel.WithAlpha(0.72f));
        UIFactory.Anchor(bar.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 0f), new Vector2(0f, 96f));

        TextMeshProUGUI livesLabel = UIFactory.Text("LivesLabel", bar.rectTransform, "VIDAS", 24f,
            Palette.Muted, TextAlignmentOptions.Left);
        UIFactory.Anchor(livesLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(36f, 0f), new Vector2(150f, 0f));

        for (int i = 0; i < GameManager.StartLives; i++)
        {
            Image heart = UIFactory.Panel("Heart" + i, bar.rectTransform, Palette.Pink);
            heart.sprite = ArtFactory.Heart;
            heart.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            heart.rectTransform.anchorMax = new Vector2(0f, 0.5f);
            heart.rectTransform.sizeDelta = new Vector2(44f, 44f);
            heart.rectTransform.anchoredPosition = new Vector2(164f + (i * 52f), 0f);
            hearts.Add(heart);
        }

        TextMeshProUGUI staminaLabel = UIFactory.Text("StaminaLabel", bar.rectTransform, "ESTAMINA", 24f,
            Palette.Muted, TextAlignmentOptions.Left);
        UIFactory.Anchor(staminaLabel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(380f, 0f), new Vector2(530f, 0f));

        staminaBar = UIFactory.Bar("StaminaBar", bar.rectTransform, new Vector2(260f, 20f),
            Palette.Green, Palette.Void.WithAlpha(0.85f));
        RectTransform staminaRect = staminaBar.rectTransform.parent as RectTransform;
        staminaRect.anchorMin = new Vector2(0f, 0.5f);
        staminaRect.anchorMax = new Vector2(0f, 0.5f);
        staminaRect.anchoredPosition = new Vector2(660f, 0f);

        dashText = UIFactory.Text("DashState", bar.rectTransform, "DASH PRONTO", 24f,
            Palette.Cyan, TextAlignmentOptions.Left);
        UIFactory.Anchor(dashText.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f),
            new Vector2(810f, 0f), new Vector2(1060f, 0f));

        hintText = UIFactory.Text("Hints", bar.rectTransform, string.Empty, 24f,
            Palette.Muted, TextAlignmentOptions.Right);
        UIFactory.Anchor(hintText.rectTransform, new Vector2(0.4f, 0f), new Vector2(1f, 1f),
            new Vector2(0f, 0f), new Vector2(-36f, 0f));
    }

    private void BuildAnnounce()
    {
        announceText = UIFactory.Text("Announce", root, string.Empty, 64f,
            Palette.Cyan, TextAlignmentOptions.Center);
        UIFactory.Anchor(announceText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        announceText.rectTransform.sizeDelta = new Vector2(1400f, 120f);
        announceText.rectTransform.anchoredPosition = new Vector2(0f, 300f);
        announceText.alpha = 0f;
    }

    private void BuildPausePanel()
    {
        Image backdrop = UIFactory.Panel("PauseBackdrop", root, Palette.Void.WithAlpha(0.78f));
        UIFactory.Stretch(backdrop.rectTransform);
        pausePanel = backdrop.rectTransform;

        Image frame = UIFactory.Frame("PauseFrame", pausePanel, new Vector2(760f, 560f),
            Palette.Panel, Palette.Cyan.WithAlpha(0.8f));

        TextMeshProUGUI title = UIFactory.Text("Title", frame.rectTransform, "PAUSA", 72f,
            Palette.Cyan, TextAlignmentOptions.Top);
        UIFactory.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -120f), new Vector2(0f, -36f));

        TextMeshProUGUI tip = UIFactory.Text("Tip", frame.rectTransform,
            "Colete os fragmentos para ganhar tempo.\nTomar dano custa vida e segundos.", 26f,
            Palette.Muted, TextAlignmentOptions.Top);
        UIFactory.Anchor(tip.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(40f, -230f), new Vector2(-40f, -140f));

        pausePrimaryButton = UIFactory.Button("CONTINUAR", frame.rectTransform,
            new Vector2(520f, 86f), Palette.Green, OnResumeClicked);
        pausePrimaryButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -40f);

        Button restart = UIFactory.Button("REINICIAR", frame.rectTransform,
            new Vector2(520f, 86f), Palette.Gold, OnRestartClicked);
        restart.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -140f);

        Button menu = UIFactory.Button("MENU INICIAL", frame.rectTransform,
            new Vector2(520f, 86f), Palette.Pink, OnMenuClicked);
        menu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -240f);

        pausePanel.gameObject.SetActive(false);
    }

    private void BuildEndPanel()
    {
        Image backdrop = UIFactory.Panel("EndBackdrop", root, Palette.Void.WithAlpha(0.86f));
        UIFactory.Stretch(backdrop.rectTransform);
        endPanel = backdrop.rectTransform;

        Image frame = UIFactory.Frame("EndFrame", endPanel, new Vector2(880f, 620f),
            Palette.Panel, Palette.Pink.WithAlpha(0.8f));

        endTitle = UIFactory.Text("Title", frame.rectTransform, "FIM", 68f,
            Palette.Pink, TextAlignmentOptions.Top);
        UIFactory.Anchor(endTitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -124f), new Vector2(0f, -34f));

        endStats = UIFactory.Text("Stats", frame.rectTransform, string.Empty, 32f,
            Palette.White, TextAlignmentOptions.Top);
        UIFactory.Anchor(endStats.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(40f, -350f), new Vector2(-40f, -140f));

        endPrimaryButton = UIFactory.Button("JOGAR DE NOVO", frame.rectTransform,
            new Vector2(560f, 88f), Palette.Green, OnRestartClicked);
        endPrimaryButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -100f);

        Button menu = UIFactory.Button("MENU INICIAL", frame.rectTransform,
            new Vector2(560f, 88f), Palette.Cyan, OnMenuClicked);
        menu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -205f);

        endPanel.gameObject.SetActive(false);
    }

    // ------------------------------------------------------------ runtime ----

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();

        if (announceTimer > 0f)
        {
            announceTimer -= Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(announceTimer / 0.6f);
            announceText.color = announceColor.WithAlpha(alpha);
            if (announceTimer <= 0f)
            {
                announceText.text = string.Empty;
            }
        }
    }

    private void Refresh()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null)
        {
            return;
        }

        scoreText.text = "PONTOS " + manager.Score.ToString("000000");
        highScoreText.text = "RECORDE " + manager.HighScore.ToString("000000");
        sectorText.text = "SETOR " + manager.Wave + "/" + GameManager.TotalWaves + " - " +
                          ArenaLayouts.NameForWave(manager.Wave);
        shardText.text = "FRAGMENTOS " + (manager.ShardsInWave - manager.ShardsLeft) + "/" +
                         manager.ShardsInWave;

        timeText.text = FormatTime(manager.TimeLeft);
        timeText.color = manager.TimeLeft <= 10f
            ? Color.Lerp(Palette.Pink, Palette.White, Mathf.PingPong(Time.unscaledTime * 3f, 1f))
            : Palette.Cyan;

        if (timeBar != null)
        {
            timeBar.fillAmount = Mathf.Clamp01(manager.TimeLeft / GameManager.MaxTime);
            timeBar.color = manager.TimeLeft <= 10f ? Palette.Pink : Palette.Cyan;
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            bool alive = i < manager.Lives;
            hearts[i].color = alive ? Palette.Pink : Palette.Void.WithAlpha(0.55f);
        }

        PlayerMovement player = PlayerLocator.Movement;
        if (player != null)
        {
            if (staminaBar != null)
            {
                staminaBar.fillAmount = player.Stamina01;
                staminaBar.color = player.Sprinting ? Palette.Gold : Palette.Green;
            }

            dashText.text = player.DashReady ? "DASH PRONTO" : "DASH RECARREGANDO";
            dashText.color = player.DashReady ? Palette.Cyan : Palette.Muted;
        }

        hintText.text = InputHub.GamepadConnected
            ? "ANALOGICO mover  |  L3 correr  |  A investida  |  START pausa"
            : "WASD mover  |  SHIFT correr  |  ESPACO investida  |  ESC pausa  |  M som";
    }

    public static string FormatTime(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float rest = seconds - (minutes * 60f);
        return minutes.ToString("00") + ":" + rest.ToString("00.0");
    }

    private void HandleStateChanged(GameState state)
    {
        pausePanel.gameObject.SetActive(state == GameState.Paused);
        endPanel.gameObject.SetActive(state == GameState.GameOver || state == GameState.Victory);

        if (state == GameState.Paused)
        {
            Select(pausePrimaryButton);
            return;
        }

        if (state != GameState.GameOver && state != GameState.Victory)
        {
            return;
        }

        GameManager manager = GameManager.Instance;
        bool victory = state == GameState.Victory;

        endTitle.text = victory ? "CIRCUITO COMPLETO" : "CONEXAO PERDIDA";
        endTitle.color = victory ? Palette.Green : Palette.Pink;

        string record = manager.NewRecord ? "\n<color=#FFC93D>NOVO RECORDE!</color>" : string.Empty;
        endStats.text =
            "SETOR ALCANCADO: " + manager.Wave + "/" + GameManager.TotalWaves + "\n" +
            "TEMPO DE PARTIDA: " + FormatTime(manager.RunTime) + "\n" +
            "TEMPO RESTANTE: " + FormatTime(manager.TimeLeft) + "\n" +
            "PONTOS: " + manager.Score.ToString("000000") + "\n" +
            "PONTUACAO FINAL: " + manager.FinalScore.ToString("000000") + "\n" +
            "RECORDE: " + manager.HighScore.ToString("000000") + record;

        Select(endPrimaryButton);
    }

    private void Select(Button button)
    {
        if (button == null || EventSystem.current == null)
        {
            return;
        }

        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    private void HandleAnnounce(string message, Color color)
    {
        announceText.text = message;
        announceColor = color;
        announceTimer = 2.2f;
        announceText.color = color;
    }

    private void HandlePopup(Vector3 worldPosition, string message, Color color)
    {
        Camera cam = Camera.main;
        if (cam == null || root == null)
        {
            return;
        }

        Vector2 screenPoint = cam.WorldToScreenPoint(worldPosition);
        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screenPoint, null, out local))
        {
            return;
        }

        TextMeshProUGUI text = UIFactory.Text("Popup", root, message, 40f, color,
            TextAlignmentOptions.Center);
        text.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        text.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        text.rectTransform.sizeDelta = new Vector2(320f, 70f);
        text.rectTransform.anchoredPosition = local;
        text.gameObject.AddComponent<UIFloatText>().Begin(0.9f);
    }

    private void OnResumeClicked()
    {
        GameManager manager = GameManager.Instance;
        if (manager != null)
        {
            manager.Resume();
        }
    }

    private void OnRestartClicked()
    {
        GameManager manager = GameManager.Instance;
        if (manager != null)
        {
            manager.Restart();
        }
    }

    private void OnMenuClicked()
    {
        GameManager manager = GameManager.Instance;
        if (manager != null)
        {
            manager.ToMenu();
        }
    }
}
