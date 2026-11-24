using System;
using System.ComponentModel;
using System.Globalization;
using HRZone.Configuration;
using HRZone.Utils;
using TMPro;
using UnityEngine;
using Zenject;
using IPALogger = IPA.Logging.Logger;

namespace HRZone;

// The menu display: the gameplay counter prefab as a world-space block with a clock line above
// it, driven by MenuHRFeed. Deliberately not an HRZoneCounter subclass: that base is welded to
// the gameplay provider, CoreGameHUDController.InitData and replay gating, none of which exist
// in the menu. Dispose only releases owned resources; the canvas is hidden with the menu scene.
internal sealed class MenuHRZone : MonoBehaviour, IInitializable, IDisposable
{
    // neutral pose baked from the maintainer's tuning pass (2026-11-24): slider zeros sit there,
    // offsets move the block relative to it; the point is the visible stack's center because the
    // canvas pivot is rebased to it, so tilting rotates the block around itself, not around a
    // point ~1.7 m below it (design D6)
    private static readonly Vector3 BLOCK_ANCHOR = new(0f, 3.06f, 4.3f);
    private static readonly Vector3 BLOCK_SCALE = Vector3.one / 150f;

    // D8 proportions: the counter node shrinks inside the canvas (the clock is a canvas sibling
    // sharing BLOCK_SCALE, so canvas scale alone cannot match one line's height to the other's);
    // the constants are dialed in during the maintainer's in-game pass
    private const float COUNTER_LOCAL_SCALE = 0.24f;
    private const float CLOCK_FONT_SIZE = 27.5f;
    // the prefab canvas is 900x600 with the Container anchored at y=+240 off its center:
    // content top is 240 + 50*COUNTER_LOCAL_SCALE, + small gap + half the clock rect
    private const float CLOCK_ANCHORED_Y = 275f;
    private static readonly Vector2 CLOCK_RECT = new(200f, 30f);

    // midpoint of the visible content inside the 600-unit-tall canvas rect (counter ~228..252,
    // clock glyphs ~265..285); the instance's pivot moves here so the stack rotates in place
    private const float STACK_CENTER_CANVAS_UNITS = 256f;
    private const float CANVAS_RECT_HEIGHT = 600f;

    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly IPALogger _logger = null!;

    [Inject]
    private readonly AssetBundleManager _assetBundleManager = null!;

    [Inject]
    private readonly MenuHRFeed _feed = null!;

    [Inject]
    private readonly IconManager _iconManager = null!;

    private CounterView? _view;

    private TextMeshProUGUI? _clockText;

    private Material? _clockMaterial;

    private int? _lastHr;

    private string _lastClock = "";

    // config-side visibility of the whole block (ModEnable); the host GameObject's own enable
    // state is layered on top of it, since the canvas is a parentless prefab instance
    private bool _canvasEnabled;

    public void Initialize()
    {
        var view = _assetBundleManager.SetupCustomCounter();
        if (view == null)
        {
            _logger.Warn("No Counter asset is loaded, the menu display is skipped!");
            return;
        }

        _view = view;
        view.Canvas.name = "HRZone Menu Display";
        view.Container.gameObject.name = "HRZone Menu Counter";
        view.Container.localScale = Vector3.one * COUNTER_LOCAL_SCALE;
        ((RectTransform)view.Canvas.transform).pivot = new Vector2(0.5f, 0.5f + STACK_CENTER_CANVAS_UNITS / CANVAS_RECT_HEIGHT);
        _clockText = SetupClock(view);

        ApplyPlacement();
        ApplyVisibility();
        ApplyClockBloom();
        ApplyFeedState(_feed.Status, _feed.CurrentHR);
        RefreshClock();

        _feed.StateChanged += ApplyFeedState;
        _config.PropertyChanged += OnConfigChanged;
        _iconManager.IconsLoaded += UpdateIcon;
    }

    public void Dispose()
    {
        _feed.StateChanged -= ApplyFeedState;
        _config.PropertyChanged -= OnConfigChanged;
        _iconManager.IconsLoaded -= UpdateIcon;

        _view?.Dispose();
        _view = null;
        _clockText = null;

        if (_clockMaterial != null)
        {
            Destroy(_clockMaterial);
            _clockMaterial = null;
        }
    }

    // SetupCustomCounter instantiates the prefab without a parent, so the canvas is not a menu
    // scene object and menu-scene deactivation does not hide it; the host GameObject is created by
    // the Menu context and does go inactive, so its enable/disable drives the canvas
    private void OnEnable() => _view?.Canvas.SetActive(_canvasEnabled);

    private void OnDisable() => _view?.Canvas.SetActive(false);

    private void Update()
    {
        if (_clockText == null || !_canvasEnabled || !_config.ShowMenuClock) return;
        RefreshClock();
    }

    // D5: the clock borrows the bundled counter font from the number it sits above and owns
    // its own material, so the NoBloom shader swap never touches the prefab font asset
    private TextMeshProUGUI SetupClock(CounterView view)
    {
        var go = new GameObject("HRZone Menu Clock", typeof(RectTransform));
        go.transform.SetParent(view.Canvas.transform, false);
        var rect = (RectTransform)go.transform;
        rect.sizeDelta = CLOCK_RECT;
        rect.anchoredPosition = new Vector2(0f, CLOCK_ANCHORED_Y);

        var text = go.AddComponent<TextMeshProUGUI>();
        text.font = view.Numbers.font;
        _clockMaterial = new Material(view.Numbers.fontSharedMaterial);
        text.fontSharedMaterial = _clockMaterial;
        text.color = Color.white;
        text.fontSize = CLOCK_FONT_SIZE;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        return text;
    }

    private void ApplyPlacement()
    {
        if (_view == null) return;
        var canvas = _view.Canvas.transform;
        // positive distance pulls the block toward the player along the world front-back axis,
        // keeping the height offset untouched (D6); the base orientation is vertical and
        // MenuCounterTilt rotates from it around the anchor
        canvas.position = BLOCK_ANCHOR + Vector3.up * _config.MenuCounterHeight - Vector3.forward * _config.MenuCounterDistance;
        canvas.rotation = Quaternion.Euler(_config.MenuCounterTilt, 0f, 0f);
        canvas.localScale = BLOCK_SCALE;
    }

    // ModEnable hides the whole block live: the Menu container installs once per game session, so
    // the settings toggle cannot unbind the widget, it can only switch it off
    private void ApplyVisibility()
    {
        if (_view == null) return;
        _canvasEnabled = _config.ModEnable;
        _view.Canvas.SetActive(_canvasEnabled && gameObject.activeInHierarchy);
        _view.Container.gameObject.SetActive(_canvasEnabled && _config.ShowMenuHeartRate);
        if (_clockText != null) _clockText.gameObject.SetActive(_canvasEnabled && _config.ShowMenuClock);
    }

    private void ApplyClockBloom()
    {
        var shader = _config.NoBloom ? RenderUtils.TextNoGlow : RenderUtils.TextGlow;
        if (shader == null || _clockMaterial == null) return;

        _clockMaterial.shader = shader;
        // TMP substitutes its own per-text instance over the assigned material; re-applying
        // to it keeps later NoBloom toggles effective, same as CounterView.ApplyBloom does
        var rendered = _clockText?.fontSharedMaterial;
        if (rendered != null && !ReferenceEquals(rendered, _clockMaterial)) rendered.shader = shader;
    }

    private void ApplyFeedState(MenuFeedStatus status, int? hr)
    {
        _lastHr = status == MenuFeedStatus.HasValue ? hr : null;

        if (_view == null) return;
        if (_lastHr.HasValue) _view.ApplyHeartRate(_lastHr.Value);
        else _view.ApplyNoValue();
    }

    private void RefreshClock()
    {
        if (_clockText == null) return;
        var now = DateTime.Now.ToString(_config.MenuClockFormat, CultureInfo.InvariantCulture);
        if (now == _lastClock) return;
        _lastClock = now;
        _clockText.text = now;
    }

    private void UpdateIcon()
    {
        _view?.ApplyIcon(_config.CustomIcon);
    }

    private void OnConfigChanged(object? sender, PropertyChangedEventArgs args)
    {
        var name = args.PropertyName;
        if (string.IsNullOrEmpty(name))
        {
            ApplyPlacement();
            ApplyVisibility();
            ApplyClockBloom();
            ApplyFeedState(_feed.Status, _feed.CurrentHR);
            UpdateIcon();
            return;
        }

        switch (name)
        {
            case nameof(PluginConfig.MenuCounterHeight):
            case nameof(PluginConfig.MenuCounterDistance):
            case nameof(PluginConfig.MenuCounterTilt):
                ApplyPlacement();
                break;
            case nameof(PluginConfig.ModEnable):
            case nameof(PluginConfig.ShowMenuClock):
            case nameof(PluginConfig.ShowMenuHeartRate):
                ApplyVisibility();
                break;
            case nameof(PluginConfig.NoBloom):
                _view?.ApplyBloom(_config.NoBloom);
                ApplyClockBloom();
                break;
            case nameof(PluginConfig.CustomIcon):
                UpdateIcon();
                break;
            case nameof(PluginConfig.Colorize):
            case nameof(PluginConfig.ColorizeIcon):
                ApplyFeedState(_feed.Status, _feed.CurrentHR);
                break;
        }
    }
}
