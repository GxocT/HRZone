using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HRZone.Utils;
using IPA.Utilities;
using IPA.Utilities.Async;
using UnityEngine;
using Zenject;
using Logger = IPA.Logging.Logger;
using UObject = UnityEngine.Object;

namespace HRZone;

internal class IconManager : IInitializable, IDisposable
{
    [Inject]
    private readonly Logger _logger = null!;

    private readonly DirectoryInfo _iconDir = new(Path.Combine(UnityGame.UserDataPath, "HRZone", "Icons"));

    private readonly Dictionary<string, Sprite> _loadedIcons = new();

    private readonly HashSet<string> _acceptableExtensions = [".png", ".jpg", ".jpeg", ".gif"];

    private static readonly (string ResourceName, string FileName)[] PresetIcons =
    [
        ("HRZone.Resources.HR-Light.png", "HR-Light.png"),
        ("HRZone.Resources.HR-Medium.png", "HR-Medium.png"),
        ("HRZone.Resources.HR-Strong.png", "HR-Strong.png"),
        ("HRZone.Resources.HRE-Light.png", "HRE-Light.png"),
        ("HRZone.Resources.HRE-Medium.png", "HRE-Medium.png"),
        ("HRZone.Resources.HRE-Strong.png", "HRE-Strong.png")
    ];

    internal string IconDirPath => _iconDir.FullName;
    
    internal Sprite? DefaultIcon { get; private set; }

    internal event Action? IconsLoaded;

    internal IconManager()
    {
        _iconDir.Create();
    }

    void IInitializable.Initialize()
    {
        DefaultIcon = LoadDefaultIcon();
        SeedPresetIcons();

        UnityMainThreadTaskScheduler.Factory.StartNew(LoadAllIconsAsync);
    }

    private void SeedPresetIcons()
    {
        foreach (var (resourceName, fileName) in PresetIcons)
        {
            var iconPath = Path.Combine(IconDirPath, fileName);
            if (File.Exists(iconPath)) continue;

            try
            {
                using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
                if (resourceStream == null)
                {
                    _logger.Warn($"Failed to find {resourceName} from ManifestResourceStream!");
                    continue;
                }

                using var fileStream = new FileStream(iconPath, FileMode.Create, FileAccess.Write);
                resourceStream.CopyTo(fileStream);
            }
            catch (Exception e)
            {
                _logger.Warn($"Failed to seed {fileName}");
                _logger.Debug(e);
            }
        }
    }

    void IDisposable.Dispose()
    {
        if (DefaultIcon != null)
        {
            var texture = DefaultIcon.texture;
            UObject.Destroy(DefaultIcon);
            UObject.Destroy(texture);
            DefaultIcon = null;
        }

        ClearIconCache();
    }

    private Sprite? LoadDefaultIcon()
    {
        using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HRZone.Resources.heart.png");
        if (resourceStream == null)
        {
            _logger.Error("Failed to find heart.png from ManifestResourceStream!");
            return null;
        }

        try
        {
            using var memoryStream = new MemoryStream();
            resourceStream.CopyTo(memoryStream);

            var texture = new Texture2D(2, 2);
            texture.LoadImage(memoryStream.ToArray());
            texture.name = "Default Icon";
            return RenderUtils.CreateSprite(texture);
        }
        catch (Exception e)
        {
            _logger.Critical("Failed to decode the default icon from heart.png");
            _logger.Critical(e);
            return null;
        }
    }

    internal bool TryGetIconSprite(string name, out Sprite sprite)
    {
        return _loadedIcons.TryGetValue(name, out sprite);
    }

    private void ClearIconCache()
    {
        foreach (var icon in _loadedIcons)
        {
            var texture = icon.Value.texture;
            UObject.Destroy(icon.Value);
            UObject.Destroy(texture);
        }

        _loadedIcons.Clear();
    }

    internal async Task<IList<Tuple<string, Sprite>>> GetIconsWithSpriteAsync(bool refresh)
    {
        if (!UnityGame.OnMainThread)
        {
            throw new InvalidOperationException("This method can only be called from the main thread.");
        }

        if (refresh)
        {
            await LoadAllIconsAsync();
        }

        return _loadedIcons.Select(icon => new Tuple<string, Sprite>(icon.Key, icon.Value)).ToList();
    }

    private async Task LoadAllIconsAsync()
    {
        _logger.Debug("Loading all icons");
        ClearIconCache();
        foreach (var file in _iconDir.EnumerateFiles())
        {
            if (file.Exists && _acceptableExtensions.Contains(file.Extension))
            {
                var name = file.Name;
                var sprite = await LoadIconSpriteAsync(file);
                if (sprite != null && !string.IsNullOrWhiteSpace(name))
                {
                    _loadedIcons[name] = sprite;
                }
            }
        }

        _logger.Debug($"Loaded {_loadedIcons.Count} icons");

        _ = UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            try
            {
                var action = IconsLoaded;
                action?.Invoke();
            }
            catch (Exception e)
            {
                _logger.Error("Failed to invoke IconsLoaded event");
                _logger.Error(e);
            }
        });
    }

    private async Task<Sprite?> LoadIconSpriteAsync(FileInfo file)
    {
        var fileName = file.Name;
        _logger.Trace($"Loading icon {fileName}");

        try
        {
            _logger.Trace($"Creating texture from {fileName}");
            var texture = await RenderUtils.LoadImageAsync(file);
            if (texture == null)
            {
                _logger.Warn($"Failed to load image from {fileName}");
                return null;
            }

            var sprite = RenderUtils.CreateSprite(texture);

            _logger.Trace($"Loaded sprite from {fileName}");
            return sprite;
        }
        catch (Exception e)
        {
            _logger.Critical($"Failed to load icon sprite from {fileName}");
            _logger.Critical(e);
            return null;
        }
    }
}
