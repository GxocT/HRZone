using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HRZone.Utils;
using HRZone.Utils.Converters;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;
using IPA.Utilities.Async;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;
using IPAConfig = IPA.Config.Config;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace HRZone.Configuration;

internal class PluginConfig : INotifyPropertyChanged
{
    private static PluginConfig? _instance;
    private static IPALogger _logger = null!;

    public static PluginConfig DefaultValues { get; } = new();

    public const string DEFAULT_CLOCK_FORMAT = "HH:mm";

    internal static readonly string[] CLOCK_FORMATS = ["HH:mm", "HH:mm:ss", "h:mm tt"];

    public static PluginConfig Initialize(IPALogger logger, IPAConfig conf)
    {
        if (_instance != null) return _instance;
        _logger = logger;
        _instance = conf.Generated<PluginConfig>();
        _logger.Debug("Config loaded");
        return _instance;
    }

    private readonly bool _isGenerateStore;

    public PluginConfig()
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        // BSIPA generated config store has this interface
        _isGenerateStore = this is IConfigStore;
        if (_isGenerateStore)
        {
            _logger.Trace("Hot Config ctor");
        }
    }

    #region private backing fields

    private bool _modEnable = true;
    private bool _showInGameCounter = true;
    private string _dataSource = "WebRequest";
    private string _feedLink = "http://127.0.0.1:62261/bpm";
    private bool _noBloom = false;
    private bool _colorize = true;
    private bool _colorizeIcon = true;
    private bool _autoHideCounter = false;
    private bool _countersPlusIntegration = false;
    private bool _showMenuClock = true;
    private bool _showMenuHeartRate = true;
    private string _menuClockFormat = DEFAULT_CLOCK_FORMAT;
    private float _menuCounterHeight = 0f;
    private float _menuCounterDistance = 0f;
    private float _menuCounterTilt = 0f;
    private int _maxHR = 180;
    private Color _zone1Color = new(0.6117647f, 0.5607843f, 0.5490196f); // default to mauve (#9C8F8C)
    private Color _zone2Color = new(0.4980392f, 0.6509804f, 0.7686275f); // default to blue (#7FA6C4)
    private Color _zone3Color = new(0.5764706f, 0.7529412f, 0.5411765f); // default to green (#93C08A)
    private Color _zone4Color = new(0.9411765f, 0.6392157f, 0.3686275f); // default to orange (#F0A35E)
    private Color _zone5Color = new(0.9058824f, 0.4156863f, 0.3686275f); // default to red (#E76A5E)
    private string _colorTheme = ColorThemeUtils.SUNSET_KEY;
    private Color _userThemeZone1Color = new(0.6117647f, 0.5607843f, 0.5490196f); // default to mauve (#9C8F8C)
    private Color _userThemeZone2Color = new(0.4980392f, 0.6509804f, 0.7686275f); // default to blue (#7FA6C4)
    private Color _userThemeZone3Color = new(0.5764706f, 0.7529412f, 0.5411765f); // default to green (#93C08A)
    private Color _userThemeZone4Color = new(0.9411765f, 0.6392157f, 0.3686275f); // default to orange (#F0A35E)
    private Color _userThemeZone5Color = new(0.9058824f, 0.4156863f, 0.3686275f); // default to red (#E76A5E)
    private Vector3 _staticCounterPosition = new(0f, 1.2f, 7f);

    private string _customIcon = "";

    #endregion

    // Must be 'virtual' if you want BSIPA to detect a value change and save the config automatically.

    public virtual bool ModEnable
    {
        get => _modEnable;
        set => _modEnable = value;
    }

    public virtual bool ShowInGameCounter
    {
        get => _showInGameCounter;
        set => _showInGameCounter = value;
    }

    public virtual string DataSource
    {
        get => _dataSource;
        set => _dataSource = value;
    }

    public virtual string FeedLink
    {
        get => _feedLink;
        set => _feedLink = value;
    }

    public virtual bool NoBloom
    {
        get => _noBloom;
        set => _noBloom = value;
    }

    public virtual bool Colorize
    {
        get => _colorize;
        set => _colorize = value;
    }

    public virtual bool ColorizeIcon
    {
        get => _colorizeIcon;
        set => _colorizeIcon = value;
    }

    public virtual bool AutoHideCounter
    {
        get => _autoHideCounter;
        set => _autoHideCounter = value;
    }

    public virtual bool CountersPlusIntegration
    {
        get => _countersPlusIntegration;
        set => _countersPlusIntegration = value;
    }

    public virtual bool ShowMenuClock
    {
        get => _showMenuClock;
        set => _showMenuClock = value;
    }

    public virtual bool ShowMenuHeartRate
    {
        get => _showMenuHeartRate;
        set => _showMenuHeartRate = value;
    }

    // a stale or hand-edited format string falls back to the default instead of breaking the clock line
    public virtual string MenuClockFormat
    {
        get => Array.IndexOf(CLOCK_FORMATS, _menuClockFormat) >= 0 ? _menuClockFormat : DEFAULT_CLOCK_FORMAT;
        set => _menuClockFormat = value;
    }

    public virtual float MenuCounterHeight
    {
        get => _menuCounterHeight;
        set => _menuCounterHeight = value;
    }

    public virtual float MenuCounterDistance
    {
        get => _menuCounterDistance;
        set => _menuCounterDistance = value;
    }

    // a rotation offset on top of the block's proven base tilt (design D6)
    public virtual float MenuCounterTilt
    {
        get => _menuCounterTilt;
        set => _menuCounterTilt = value;
    }

    public virtual int MaxHR
    {
        get => _maxHR;
        set => _maxHR = value;
    }

    [UseConverter(typeof(HexColorConverter))]
    [JsonConverter(typeof(WrappedTextValueJsonConverter<Color, HexColorConverter>))]
    public virtual Color Zone1Color
    {
        get => _zone1Color;
        set => _zone1Color = value;
    }

    [UseConverter(typeof(HexColorConverter))]
    [JsonConverter(typeof(WrappedTextValueJsonConverter<Color, HexColorConverter>))]
    public virtual Color Zone2Color
    {
        get => _zone2Color;
        set => _zone2Color = value;
    }

    [UseConverter(typeof(HexColorConverter))]
    [JsonConverter(typeof(WrappedTextValueJsonConverter<Color, HexColorConverter>))]
    public virtual Color Zone3Color
    {
        get => _zone3Color;
        set => _zone3Color = value;
    }

    [UseConverter(typeof(HexColorConverter))]
    [JsonConverter(typeof(WrappedTextValueJsonConverter<Color, HexColorConverter>))]
    public virtual Color Zone4Color
    {
        get => _zone4Color;
        set => _zone4Color = value;
    }

    [UseConverter(typeof(HexColorConverter))]
    [JsonConverter(typeof(WrappedTextValueJsonConverter<Color, HexColorConverter>))]
    public virtual Color Zone5Color
    {
        get => _zone5Color;
        set => _zone5Color = value;
    }

    public virtual string ColorTheme
    {
        get => _colorTheme;
        set => _colorTheme = value;
    }

    [UseConverter(typeof(HexColorConverter))]
    [JsonConverter(typeof(WrappedTextValueJsonConverter<Color, HexColorConverter>))]
    public virtual Color UserThemeZone1Color
    {
        get => _userThemeZone1Color;
        set => _userThemeZone1Color = value;
    }

    [UseConverter(typeof(HexColorConverter))]
    [JsonConverter(typeof(WrappedTextValueJsonConverter<Color, HexColorConverter>))]
    public virtual Color UserThemeZone2Color
    {
        get => _userThemeZone2Color;
        set => _userThemeZone2Color = value;
    }

    [UseConverter(typeof(HexColorConverter))]
    [JsonConverter(typeof(WrappedTextValueJsonConverter<Color, HexColorConverter>))]
    public virtual Color UserThemeZone3Color
    {
        get => _userThemeZone3Color;
        set => _userThemeZone3Color = value;
    }

    [UseConverter(typeof(HexColorConverter))]
    [JsonConverter(typeof(WrappedTextValueJsonConverter<Color, HexColorConverter>))]
    public virtual Color UserThemeZone4Color
    {
        get => _userThemeZone4Color;
        set => _userThemeZone4Color = value;
    }

    [UseConverter(typeof(HexColorConverter))]
    [JsonConverter(typeof(WrappedTextValueJsonConverter<Color, HexColorConverter>))]
    public virtual Color UserThemeZone5Color
    {
        get => _userThemeZone5Color;
        set => _userThemeZone5Color = value;
    }

    [JsonConverter(typeof(Vector3JsonConverter))]
    public virtual Vector3 StaticCounterPosition
    {
        get => _staticCounterPosition;
        set => _staticCounterPosition = value;
    }

    public virtual string CustomIcon
    {
        get => _customIcon;
        set => _customIcon = value;
    }

    protected virtual void Changed()
    {
        _logger.Trace("Changed");
        // generated config will raise property changed events
        if (!_isGenerateStore) RaisePropertyChanged(null);
    }

    [UsedImplicitly]
    protected virtual void OnReload()
    {
        _logger.Trace("OnReload");
        // generated config will raise property changed events
        if (!_isGenerateStore) RaisePropertyChanged(null);
    }

    protected internal virtual void CopyFrom(PluginConfig other)
    {
        _logger.Trace("CopyFrom");
        // generated config has generated copy logic, and will raise property changed events
        if (!_isGenerateStore) CopyFromInternal(other);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    [UsedImplicitly]
    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        _logger.Spam($"Property Changed: {propertyName}");
        var @event = PropertyChanged;
        if (@event == null) return;
        var args = new PropertyChangedEventArgs(propertyName);

        UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            foreach (var handler in @event.GetInvocationList())
            {
                try
                {
                    ((PropertyChangedEventHandler)handler)(this, args);
                }
                catch (Exception e)
                {
                    _logger.Critical(e);
                }
            }
        });
    }

    private void CopyFromInternal(PluginConfig other)
    {
        _modEnable = other._modEnable;
        _showInGameCounter = other._showInGameCounter;
        _dataSource = other._dataSource;
        _feedLink = other._feedLink;
        _noBloom = other._noBloom;
        _colorize = other._colorize;
        _colorizeIcon = other._colorizeIcon;
        _autoHideCounter = other._autoHideCounter;
        _countersPlusIntegration = other._countersPlusIntegration;
        _showMenuClock = other._showMenuClock;
        _showMenuHeartRate = other._showMenuHeartRate;
        _menuClockFormat = other._menuClockFormat;
        _menuCounterHeight = other._menuCounterHeight;
        _menuCounterDistance = other._menuCounterDistance;
        _menuCounterTilt = other._menuCounterTilt;
        _maxHR = other._maxHR;
        _zone1Color = other._zone1Color;
        _zone2Color = other._zone2Color;
        _zone3Color = other._zone3Color;
        _zone4Color = other._zone4Color;
        _zone5Color = other._zone5Color;
        _colorTheme = other._colorTheme;
        _userThemeZone1Color = other._userThemeZone1Color;
        _userThemeZone2Color = other._userThemeZone2Color;
        _userThemeZone3Color = other._userThemeZone3Color;
        _userThemeZone4Color = other._userThemeZone4Color;
        _userThemeZone5Color = other._userThemeZone5Color;
        _staticCounterPosition = other._staticCounterPosition;
        _customIcon = other._customIcon;

        Changed();
    }
}
