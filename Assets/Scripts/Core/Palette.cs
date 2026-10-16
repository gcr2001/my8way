using UnityEngine;

/// <summary>
/// Paleta unica do tema "Neon Circuit". Todos os sprites, HUD e luzes do jogo
/// usam estas cores para manter a identidade visual coerente.
/// </summary>
public static class Palette
{
    public static readonly Color Void = FromHex("05070F");
    public static readonly Color Deep = FromHex("0A0E1A");
    public static readonly Color Panel = FromHex("101628");
    public static readonly Color PanelLight = FromHex("1B2440");
    public static readonly Color Cyan = FromHex("3DF2FF");
    public static readonly Color Green = FromHex("39FF88");
    public static readonly Color Gold = FromHex("FFC93D");
    public static readonly Color Pink = FromHex("FF4D6D");
    public static readonly Color Purple = FromHex("C04DFF");
    public static readonly Color Orange = FromHex("FF8A3D");
    public static readonly Color White = FromHex("F0FAFF");
    public static readonly Color Muted = FromHex("7C8AA8");

    /// <summary>Converte "RRGGBB" ou "RRGGBBAA" em Color.</summary>
    public static Color FromHex(string hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return Color.magenta;
        }

        if (hex[0] == '#')
        {
            hex = hex.Substring(1);
        }

        if (hex.Length != 6 && hex.Length != 8)
        {
            return Color.magenta;
        }

        float r = ByteAt(hex, 0) / 255f;
        float g = ByteAt(hex, 2) / 255f;
        float b = ByteAt(hex, 4) / 255f;
        float a = hex.Length == 8 ? ByteAt(hex, 6) / 255f : 1f;
        return new Color(r, g, b, a);
    }

    /// <summary>Mesma cor com outro alpha.</summary>
    public static Color WithAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    /// <summary>Clareia (t &gt; 0) ou escurece (t &lt; 0) uma cor.</summary>
    public static Color Shade(this Color color, float t)
    {
        Color target = t >= 0f ? Color.white : Color.black;
        float amount = Mathf.Abs(t);
        Color result = Color.Lerp(color, target, Mathf.Clamp01(amount));
        result.a = color.a;
        return result;
    }

    private static int ByteAt(string hex, int index)
    {
        return (HexDigit(hex[index]) << 4) | HexDigit(hex[index + 1]);
    }

    private static int HexDigit(char c)
    {
        if (c >= '0' && c <= '9')
        {
            return c - '0';
        }

        if (c >= 'a' && c <= 'f')
        {
            return 10 + (c - 'a');
        }

        if (c >= 'A' && c <= 'F')
        {
            return 10 + (c - 'A');
        }

        return 0;
    }
}
