using System.Collections.Generic;
using UnityEngine;

namespace HRZone.Utils;

internal static class ColorThemeUtils
{
    internal const string SUNSET_KEY = "Sunset";
    internal const string PASTEL_KEY = "Pastel";
    internal const string BRIGHT_KEY = "Bright";
    internal const string USER_THEME_KEY = "User Theme";

    private static readonly IReadOnlyDictionary<string, Color[]> PRESETS = new Dictionary<string, Color[]>
    {
        [SUNSET_KEY] = [FromHex("9C8F8C"), FromHex("7FA6C4"), FromHex("93C08A"), FromHex("F0A35E"), FromHex("E76A5E")],
        [PASTEL_KEY] = [FromHex("9AA3AF"), FromHex("7FB6DD"), FromHex("86CBA6"), FromHex("F0CB74"), FromHex("EE8F7A")],
        [BRIGHT_KEY] = [FromHex("909090"), FromHex("2FA0FF"), FromHex("00FF00"), FromHex("FFFF00"), FromHex("FF0000")],
    };

    public static IReadOnlyList<string> DROPDOWN_KEYS { get; } = [SUNSET_KEY, PASTEL_KEY, BRIGHT_KEY, USER_THEME_KEY];

    public static bool TryGetPreset(string key, out Color[] colors)
    {
        if (PRESETS.TryGetValue(key, out var preset))
        {
            colors = preset;
            return true;
        }

        colors = [];
        return false;
    }

    // Byte-level on purpose: config colors round-trip through the hex the HexColorConverter writes,
    // so float Color equality would be the wrong comparison.
    public static bool ZoneColorsMatch(Color[] colors, IReadOnlyList<Color> preset)
    {
        for (var i = 0; i < colors.Length; i++)
        {
            Color32 actual = colors[i];
            Color32 expected = preset[i];
            if (actual.r != expected.r || actual.g != expected.g || actual.b != expected.b) return false;
        }

        return true;
    }

    private static Color FromHex(string hex)
    {
        ColorUtility.TryParseHtmlString($"#{hex}", out var color);
        return color;
    }
}
