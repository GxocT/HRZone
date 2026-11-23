using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using HRZone.Configuration;
using UnityEngine;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;

namespace HRZone.Utils;

public static class RenderUtils
{
    private static readonly Lazy<Material?> _uiNoGlow = new(() => FindGameRenderObject<Material>("UINoGlow", "UI material"));

    private static readonly Lazy<Shader?> _textNoGlow = new(() =>
        FindGameRenderObject<Shader>("TextMeshPro/Mobile/Distance Field Zero Alpha Write", "no-glow text shader"));

    private static readonly Lazy<Shader?> _textGlow = new(() =>
        FindGameRenderObject<Shader>("TextMeshPro/Distance Field", "glow text shader"));

    public static Material? UINoGlow => _uiNoGlow.Value;
    public static Shader? TextNoGlow => _textNoGlow.Value;
    public static Shader? TextGlow => _textGlow.Value;

    private static T? FindGameRenderObject<T>(string name, string description) where T : UnityEngine.Object
    {
        // Exact names: the mod's own asset bundle ships stock TMP shaders, so Contains(...) could
        // match a variant (e.g. "Distance Field Overlay") and pick non-deterministically.
        var found = Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(x => x.name == name);
        if (found == null)
            Plugin.Logger.Warn($"Could not find the game {description} '{name}'; the counter may render without the intended glow treatment");
        return found;
    }

    private static PluginConfig Config => Plugin.Config;


    internal static Color DetermineZoneColor(int hr)
    {
        var hrMax = Config.MaxHR;
        if (hr < hrMax * 6 / 10) return Config.Zone1Color;
        if (hr < hrMax * 7 / 10) return Config.Zone2Color;
        if (hr < hrMax * 8 / 10) return Config.Zone3Color;
        if (hr < hrMax * 9 / 10) return Config.Zone4Color;
        return Config.Zone5Color;
    }

    internal static async Task<Texture2D?> LoadImageAsync(FileInfo file)
    {
        if (!file.Exists || file.Length == 0)
        {
            return null;
        }

        await using var stream = file.OpenRead();

#if DEBUG
        var stopwatch = new Stopwatch();
        stopwatch.Start();
#endif
        // BSML's decoder makes the texture non-readable by default, which GetPixels32 in
        // PrepareIconTexture cannot consume; mips are skipped because PrepareIconTexture
        // rebuilds the mip chain itself.
        var texture = await Utilities.LoadImageAsync(stream, updateMipmaps: false, makeNoLongerReadable: false);

#if DEBUG
        stopwatch.Stop();
        Plugin.Logger.Trace($"LoadImageAsync took {stopwatch.ElapsedMilliseconds}ms to load {file.Name}");
#endif
        texture.name = file.Name;
        return texture;
    }

    internal static Sprite CreateSprite(Texture2D texture)
    {
        var prepared = PrepareIconTexture(texture);
        var sprite = Sprite.Create(prepared, new Rect(0, 0, prepared.width, prepared.height), new Vector2(0.5f, 0.5f));
        sprite.name = prepared.name;
        return sprite;
    }

    private static Texture2D PrepareIconTexture(Texture2D decoded)
    {
        // Bilinear filtering blends RGB and alpha independently, so dark premultiplied-style
        // fringes appear once the icon is tinted; white RGB under any partly transparent pixel kills them.
        var pixels = decoded.GetPixels32();
        for (var i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a >= 255) continue;
            pixels[i].r = 255;
            pixels[i].g = 255;
            pixels[i].b = 255;
        }

        var mipCapable = IsPowerOfTwo(decoded.width) && IsPowerOfTwo(decoded.height);
        if (!mipCapable)
        {
            decoded.SetPixels32(pixels);
            decoded.Apply(false);
            Plugin.Logger.Debug($"Skipped mipmaps for icon '{decoded.name}' ({decoded.width}x{decoded.height}): texture size is not a power of two");
            return decoded;
        }

        // Textures decoded at runtime carry no mip chain and the game's Unity cannot generate
        // one in place, so copy into a mip-mapped texture; Apply(true) fills the chain.
        var texture = new Texture2D(decoded.width, decoded.height, decoded.format, true, false)
        {
            name = decoded.name,
            wrapMode = decoded.wrapMode
        };
        texture.SetPixels32(pixels);
        texture.Apply(true);
        Object.Destroy(decoded);
        return texture;
    }

    private static bool IsPowerOfTwo(int size) => size > 0 && (size & (size - 1)) == 0;
}
