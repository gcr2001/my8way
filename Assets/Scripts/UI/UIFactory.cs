using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Funcoes auxiliares para montar a interface por codigo: canvas, paineis,
/// textos, barras e botoes no estilo neon do jogo. Tambem garante um
/// EventSystem compativel com o Input System (necessario para navegar o menu
/// com teclado e com controle).
/// </summary>
public static class UIFactory
{
    public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

    /// <summary>Cria (ou reaproveita) um canvas em overlay com escala por resolucao.</summary>
    public static Canvas CreateCanvas(string name, int sortingOrder)
    {
        GameObject go = new GameObject(name);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();
        return canvas;
    }

    /// <summary>
    /// Garante um EventSystem usando o modulo do Input System. Sem isso a UI
    /// nao responde a mouse, teclado ou controle quando o projeto usa somente
    /// o pacote novo de input.
    /// </summary>
    public static void EnsureEventSystem()
    {
        EventSystem existing = Object.FindAnyObjectByType<EventSystem>();
        if (existing != null)
        {
            // Ja existe (as cenas do tutorial trazem um). Se por algum motivo
            // estiver sem modulo de input, completa com o do Input System.
            if (existing.GetComponent<BaseInputModule>() == null)
            {
                existing.gameObject.AddComponent<InputSystemUIInputModule>();
            }

            return;
        }

        GameObject host = new GameObject("EventSystem");
        host.AddComponent<EventSystem>();
        host.AddComponent<InputSystemUIInputModule>();
    }

    public static RectTransform Rect(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.SetParent(parent, false);

        // Ancoragem central por padrao: posicionar com anchoredPosition fica
        // previsivel, e quem precisa de outra ancora chama Anchor/Stretch.
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;
        return rect;
    }

    /// <summary>Retangulo colorido (usa o sprite "pixel" para ficar nitido).</summary>
    public static Image Panel(string name, Transform parent, Color color)
    {
        RectTransform rect = Rect(name, parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = ArtFactory.Pixel;
        image.type = Image.Type.Simple;
        image.color = color;
        return image;
    }

    public static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    /// <summary>Posiciona um elemento com ancora, pivo, posicao e tamanho explicitos.</summary>
    public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot,
        Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    public static void Stretch(RectTransform rect, float padding = 0f)
    {
        Anchor(rect, Vector2.zero, Vector2.one,
            new Vector2(padding, padding), new Vector2(-padding, -padding));
    }

    public static TextMeshProUGUI Text(string name, Transform parent, string content, float size,
        Color color, TextAlignmentOptions alignment)
    {
        RectTransform rect = Rect(name, parent);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    /// <summary>Caixa com borda neon, usada nos paineis de pausa e fim de jogo.</summary>
    public static Image Frame(string name, Transform parent, Vector2 size, Color fill, Color border)
    {
        Image panel = Panel(name, parent, fill);
        RectTransform rect = panel.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        Image edge = Panel("Border", rect, border);
        Stretch(edge.rectTransform, -3f);
        edge.transform.SetAsFirstSibling();
        return panel;
    }

    /// <summary>Botao neon com texto; devolve o componente Button configurado.</summary>
    public static Button Button(string label, Transform parent, Vector2 size, Color accent,
        UnityEngine.Events.UnityAction onClick)
    {
        Image background = Panel("Button_" + label, parent, Palette.PanelLight);
        RectTransform rect = background.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        Image edge = Panel("Edge", rect, accent.WithAlpha(0.85f));
        Stretch(edge.rectTransform, -2f);
        edge.transform.SetAsFirstSibling();

        TextMeshProUGUI text = Text("Label", rect, label, 38f, accent, TextAlignmentOptions.Center);
        Stretch(text.rectTransform);

        Button button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.5f, 1.5f, 1.5f, 1f);
        colors.selectedColor = new Color(1.5f, 1.5f, 1.5f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        if (onClick != null)
        {
            button.onClick.AddListener(onClick);
        }

        return button;
    }

    /// <summary>Barra de progresso simples (vida, estamina, tempo).</summary>
    public static Image Bar(string name, Transform parent, Vector2 size, Color fill, Color back)
    {
        Image background = Panel(name, parent, back);
        background.rectTransform.sizeDelta = size;

        Image bar = Panel("Fill", background.rectTransform, fill);
        Anchor(bar.rectTransform, Vector2.zero, Vector2.one, new Vector2(2f, 2f), new Vector2(-2f, -2f));
        bar.type = Image.Type.Filled;
        bar.fillMethod = Image.FillMethod.Horizontal;
        bar.fillAmount = 1f;
        return bar;
    }
}
