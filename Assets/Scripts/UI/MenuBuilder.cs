using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Monta o menu inicial com o mesmo tema da partida (mesma paleta, mesmos
/// sprites e a mesma trilha), incluindo instrucoes de como jogar e creditos.
/// </summary>
public class MenuBuilder : MonoBehaviour
{
    private MenuActions actions;
    private RectTransform helpPanel;
    private RectTransform creditsPanel;
    private Button playButton;

    public static MenuBuilder Build()
    {
        GameObject host = new GameObject("MainMenu");
        return host.AddComponent<MenuBuilder>();
    }

    private void Awake()
    {
        actions = gameObject.AddComponent<MenuActions>();

        SetupCamera();
        ReplaceExistingCanvases();

        Canvas canvas = UIFactory.CreateCanvas("MenuCanvas", 50);
        RectTransform root = canvas.GetComponent<RectTransform>();

        BuildBackground(root);
        BuildTitle(root);
        BuildButtons(root);
        helpPanel = BuildHelpPanel(root);
        creditsPanel = BuildCreditsPanel(root);

        helpPanel.gameObject.SetActive(false);
        creditsPanel.gameObject.SetActive(false);

        if (playButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }
    }

    private void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
        }

        cam.orthographic = true;
        cam.orthographicSize = 5.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Palette.Deep;
        cam.transform.position = new Vector3(0f, 0f, -10f);
    }

    /// <summary>
    /// O menu do tutorial e substituido por um menu coerente com o jogo. O
    /// canvas antigo e removido para nao sobrepor a nova interface.
    /// </summary>
    private void ReplaceExistingCanvases()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>();
        for (int i = 0; i < canvases.Length; i++)
        {
            if (canvases[i] != null)
            {
                canvases[i].gameObject.SetActive(false);
                Destroy(canvases[i].gameObject);
            }
        }
    }

    private void BuildBackground(RectTransform root)
    {
        // O fundo e a propria cor da camera: assim as luzes e o mascote (que
        // vivem no mundo) continuam visiveis atras da interface.
        Decor.Build(transform);

        SpriteRenderer mascot = ArtFactory.NewSprite("Mascot", ArtFactory.Player, Color.white,
            transform, new Vector3(-6.6f, -0.4f, 0f), 5);
        mascot.transform.localScale = Vector3.one * 2.4f;
        mascot.gameObject.AddComponent<MenuSpin>();

        SpriteRenderer shard = ArtFactory.NewSprite("MenuShard", ArtFactory.Shard, Color.white,
            transform, new Vector3(6.6f, 0.2f, 0f), 5);
        shard.transform.localScale = Vector3.one * 1.8f;
        shard.gameObject.AddComponent<MenuSpin>();
    }

    private void BuildTitle(RectTransform root)
    {
        TextMeshProUGUI title = UIFactory.Text("Title", root, "MY 8 WAY", 150f,
            Palette.Cyan, TextAlignmentOptions.Center);
        UIFactory.Anchor(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -240f), new Vector2(0f, -70f));

        TextMeshProUGUI subtitle = UIFactory.Text("Subtitle", root, "NEON CIRCUIT RUN", 46f,
            Palette.Green, TextAlignmentOptions.Center);
        UIFactory.Anchor(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -310f), new Vector2(0f, -240f));

        TextMeshProUGUI pitch = UIFactory.Text("Pitch", root,
            "Colete os fragmentos de dados antes que o tempo zere.", 30f,
            Palette.Muted, TextAlignmentOptions.Center);
        UIFactory.Anchor(pitch.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -368f), new Vector2(0f, -312f));

        int record = PlayerPrefs.GetInt("my8way_highscore", 0);
        TextMeshProUGUI best = UIFactory.Text("Record", root,
            "RECORDE " + record.ToString("000000"), 32f, Palette.Gold, TextAlignmentOptions.Center);
        UIFactory.Anchor(best.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 150f), new Vector2(0f, 206f));

        TextMeshProUGUI footer = UIFactory.Text("Footer", root,
            "Projeto Introducao ao Unity  |  M liga/desliga o som", 24f,
            Palette.Muted, TextAlignmentOptions.Center);
        UIFactory.Anchor(footer.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
            new Vector2(0f, 40f), new Vector2(0f, 90f));
    }

    private void BuildButtons(RectTransform root)
    {
        RectTransform column = UIFactory.Rect("Buttons", root);
        UIFactory.Anchor(column, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero);
        column.anchoredPosition = new Vector2(0f, -120f);

        playButton = UIFactory.Button("JOGAR", column, new Vector2(520f, 92f), Palette.Green,
            actions.IniciaJogo);
        playButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 60f);

        Button help = UIFactory.Button("COMO JOGAR", column, new Vector2(520f, 82f), Palette.Cyan,
            ToggleHelp);
        help.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -45f);

        Button credits = UIFactory.Button("CREDITOS", column, new Vector2(520f, 82f), Palette.Gold,
            ToggleCredits);
        credits.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -140f);

        if (Application.platform != RuntimePlatform.WebGLPlayer)
        {
            Button quit = UIFactory.Button("SAIR", column, new Vector2(520f, 82f), Palette.Pink,
                actions.Sair);
            quit.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -235f);
        }
    }

    private RectTransform BuildHelpPanel(RectTransform root)
    {
        RectTransform panel = BuildOverlay(root, "HelpPanel", "COMO JOGAR",
            "OBJETIVO\n" +
            "Cada setor tem fragmentos de dados. Colete todos antes do cronometro zerar.\n\n" +
            "O TEMPO E O RECURSO\n" +
            "Fragmento: +3,5s   |   Setor limpo: +12s   |   Dano: -1 vida e -5s\n" +
            "A partida acaba quando o tempo zera ou quando as 3 vidas acabam.\n\n" +
            "AMEACAS\n" +
            "Sentinelas patrulham em linha reta. Cacadores perseguem quando te enxergam.\n" +
            "Nos de energia ficam parados e machucam ao serem atravessados.\n\n" +
            "TECLADO\n" +
            "WASD ou setas: mover em 8 direcoes   |   SHIFT: correr (gasta estamina)\n" +
            "ESPACO: investida   |   ESC ou P: pausa   |   M: liga/desliga o som\n\n" +
            "CONTROLE (XBOX / JOYSTICK)\n" +
            "Analogico esquerdo: mover   |   L3: correr   |   A: investida   |   START: pausa");
        return panel;
    }

    private RectTransform BuildCreditsPanel(RectTransform root)
    {
        RectTransform panel = BuildOverlay(root, "CreditsPanel", "CREDITOS",
            "PROGRAMACAO, ARTE E LEVEL DESIGN\n" +
            "Feitos para este projeto. Os sprites sao gerados por\n" +
            "Tools/generate_sprites.py (pixel art proprio, nenhum asset de terceiros).\n\n" +
            "TRILHA SONORA\n" +
            "Sintetizada em tempo de execucao por Assets/Scripts/Audio/AudioSynth.cs\n" +
            "(ondas quadrada, triangular e ruido - estilo chiptune).\n\n" +
            "EFEITO DE COLETA\n" +
            "coin-recieved.mp3 - efeito retro de moeda obtido em banco de sons livre.\n" +
            "O link completo da fonte esta no README.md do repositorio.\n\n" +
            "FERRAMENTAS\n" +
            "Unity 6 (URP 2D), TextMesh Pro e Input System.\n" +
            "Base inicial: tutorial \"Introducao Rapida\" da disciplina.");
        return panel;
    }

    private RectTransform BuildOverlay(RectTransform root, string name, string title, string body)
    {
        Image backdrop = UIFactory.Panel(name, root, Palette.Void.WithAlpha(0.92f));
        UIFactory.Stretch(backdrop.rectTransform);

        Image frame = UIFactory.Frame("Frame", backdrop.rectTransform, new Vector2(1280f, 800f),
            Palette.Panel, Palette.Cyan.WithAlpha(0.7f));

        TextMeshProUGUI heading = UIFactory.Text("Heading", frame.rectTransform, title, 64f,
            Palette.Cyan, TextAlignmentOptions.Top);
        UIFactory.Anchor(heading.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
            new Vector2(0f, -110f), new Vector2(0f, -30f));

        TextMeshProUGUI text = UIFactory.Text("Body", frame.rectTransform, body, 27f,
            Palette.White, TextAlignmentOptions.TopLeft);
        UIFactory.Anchor(text.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
            new Vector2(60f, 130f), new Vector2(-60f, -130f));

        Button close = UIFactory.Button("VOLTAR", frame.rectTransform, new Vector2(360f, 78f),
            Palette.Green, () => CloseOverlay(backdrop.rectTransform));
        close.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0f);
        close.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0f);
        close.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 70f);

        return backdrop.rectTransform;
    }

    private void ToggleHelp()
    {
        AudioManager.Instance.Click();
        creditsPanel.gameObject.SetActive(false);
        helpPanel.gameObject.SetActive(true);
    }

    private void ToggleCredits()
    {
        AudioManager.Instance.Click();
        helpPanel.gameObject.SetActive(false);
        creditsPanel.gameObject.SetActive(true);
    }

    private void CloseOverlay(RectTransform panel)
    {
        AudioManager.Instance.Click();
        panel.gameObject.SetActive(false);
        if (playButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(playButton.gameObject);
        }
    }

    private void Update()
    {
        // ESC fecha os paineis abertos (nunca deixa o jogador travado).
        if (!InputHub.PausePressed)
        {
            if (InputHub.MutePressed)
            {
                AudioManager.Instance.ToggleMute();
            }

            return;
        }

        if (helpPanel.gameObject.activeSelf || creditsPanel.gameObject.activeSelf)
        {
            helpPanel.gameObject.SetActive(false);
            creditsPanel.gameObject.SetActive(false);
            AudioManager.Instance.Click();
        }
    }
}
