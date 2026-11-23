using System;
using HRZone.Configuration;
using HRZone.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HRZone;

internal sealed class CounterView : IDisposable
{
    private const string NO_VALUE_TEXT = "--";

    public GameObject Canvas { get; }

    public Transform Container { get; }

    public TMP_Text Numbers { get; }

    public Image Icon { get; }

    private readonly IconManager _iconManager;

    private readonly PluginConfig _config;

    private readonly Material _numberMaterial;

    private bool _disposed;

    internal CounterView(GameObject canvas, Transform container, TMP_Text numbers, Image icon, IconManager iconManager, PluginConfig config)
    {
        Canvas = canvas;
        Container = container;
        Numbers = numbers;
        Icon = icon;
        _iconManager = iconManager;
        _config = config;

        _numberMaterial = new Material(numbers.fontSharedMaterial);
        numbers.fontSharedMaterial = _numberMaterial;
        Plugin.Logger.Debug($"CounterView created: owned number material id={_numberMaterial.GetInstanceID()} shader='{_numberMaterial.shader.name}'");
    }

    public void ApplyHeartRate(int hr)
    {
        var zoneColor = RenderUtils.DetermineZoneColor(hr);
        Numbers.text = hr.ToString();
        Numbers.color = _config.Colorize ? zoneColor : Color.white;
        Icon.color = _config.ColorizeIcon ? zoneColor : Color.white;
    }

    public void ApplyNoValue()
    {
        Numbers.text = NO_VALUE_TEXT;
        Numbers.color = Color.white;
        Icon.color = Color.white;
    }

    public void ApplyBloom(bool noBloom)
    {
        // No override leaves the prefab's default UI material, which the game's bloom pass picks up;
        // if UINoGlow ever fails to resolve the icon simply cannot be pinned flat (RenderUtils warns once).
        Icon.material = noBloom ? RenderUtils.UINoGlow : null;

        var shader = noBloom ? RenderUtils.TextNoGlow : RenderUtils.TextGlow;
        if (shader == null) return;

        _numberMaterial.shader = shader;
        // TMP substitutes its own per-text instance over the assigned material; it belongs to
        // this text alone, so re-applying to it is safe and keeps later toggles effective.
        var rendered = Numbers.fontSharedMaterial;
        if (rendered != null && !ReferenceEquals(rendered, _numberMaterial)) rendered.shader = shader;
        Plugin.Logger.Debug($"CounterView bloom noBloom={noBloom}: shader='{shader.name}' rendered material id={rendered?.GetInstanceID() ?? -1}");
    }

    public void ApplyIcon(string? customIcon)
    {
        Icon.sprite = string.IsNullOrWhiteSpace(customIcon)
            ? _iconManager.DefaultIcon
            : _iconManager.TryGetIconSprite(customIcon!, out var sprite)
                ? sprite
                : _iconManager.DefaultIcon;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        Object.Destroy(_numberMaterial);
        var rendered = Numbers.fontSharedMaterial;
        if (rendered != null && !ReferenceEquals(rendered, _numberMaterial) && !ReferenceEquals(rendered, Numbers.font?.material))
        {
            Object.Destroy(rendered);
        }
    }
}
