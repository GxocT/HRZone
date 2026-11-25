using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BGLib.Polyglot;
using HMUI;
using HRZone.Configuration;
using HRZone.Data;
using HRZone.Data.DataSources;
using HRZone.Utils;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using Logger = IPA.Logging.Logger;

namespace HRZone.UI;

[HotReload(RelativePathToLayout = @"BSML\configMenu.bsml")]
[ViewDefinition("HRZone.UI.BSML.configMenu.bsml")]
internal class MainConfigMenu : BaseConfigViewController
{
    [Inject]
    private readonly Logger _logger = null!;

    [Inject]
    private readonly IconManager _iconManager = null!;

    [Inject]
    private readonly DataSourceManager _dataSourceManager = null!;

    [Inject]
    private readonly MenuHRFeed _feed = null!;

    private HRZoneFeedPreview? _feedPreviewComponent;

    [Inject]
    [UsedImplicitly]
    private void Init(AssetBundleManager abm, DiContainer di)
    {
        _logger.Trace("MainConfigMenu Init");
        SetupPreviewCounters(abm, di);
    }

    protected override void OnParsed()
    {
        base.OnParsed();
        _ = LoadIconList(false);
    }

    protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
    {
        _feed.StateChanged -= ApplyFeedState;

        base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
    }

    protected override void RefreshUI()
    {
        ReconcileThemeSelection();
        UpdateCustomIconSelection();
        NotifyZoneLabels();
        AttachFeedPreview();
    }

    protected override void OnConfigChanged(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(Config.CustomIcon):
                UpdateCustomIconSelection();
                break;
            case nameof(Config.MaxHR):
                NotifyZoneLabels();
                break;
            case nameof(Config.ColorTheme):
                ApplyTheme(Config.ColorTheme);
                break;
            case nameof(Config.DataSource):
                NotifyPropertyChanged(nameof(IsWebRequestSource));
                break;
        }
    }

    #region General Tab

    private const string COUNTERS_PLUS_MOD_ID = "Counters+";

    [UIValue(nameof(Config.ModEnable))]
    public bool ModEnable
    {
        get => Config.ModEnable;
        set
        {
            if (Config.ModEnable != value) Config.ModEnable = value;
        }
    }

    [UIValue(nameof(Config.ShowInGameCounter))]
    public bool ShowInGameCounter
    {
        get => Config.ShowInGameCounter;
        set
        {
            if (Config.ShowInGameCounter != value) Config.ShowInGameCounter = value;
        }
    }

    [UIValue(nameof(Config.NoBloom))]
    public bool NoBloom
    {
        get => Config.NoBloom;
        set
        {
            if (Config.NoBloom != value) Config.NoBloom = value;
        }
    }

    [UIValue(nameof(Config.AutoHideCounter))]
    public bool AutoHideCounter
    {
        get => Config.AutoHideCounter;
        set
        {
            if (Config.AutoHideCounter != value) Config.AutoHideCounter = value;
        }
    }

    [UIValue(nameof(Config.CountersPlusIntegration))]
    public bool CountersPlusIntegration
    {
        get => Config.CountersPlusIntegration;
        set
        {
            if (Config.CountersPlusIntegration != value) Config.CountersPlusIntegration = value;
        }
    }

    // static per game session: BSIPA loads plugins before this screen can open
    [UIValue(nameof(IsCountersPlusPresent))]
    [UsedImplicitly]
    public bool IsCountersPlusPresent { get; } = Utils.Utils.IsModEnabled(COUNTERS_PLUS_MOD_ID);

    #endregion

    #region Menu Tab

    [UIValue(nameof(Config.ShowMenuClock))]
    public bool ShowMenuClock
    {
        get => Config.ShowMenuClock;
        set
        {
            if (Config.ShowMenuClock != value) Config.ShowMenuClock = value;
        }
    }

    [UIValue(nameof(Config.ShowMenuHeartRate))]
    public bool ShowMenuHeartRate
    {
        get => Config.ShowMenuHeartRate;
        set
        {
            if (Config.ShowMenuHeartRate != value) Config.ShowMenuHeartRate = value;
        }
    }

    [UIValue(nameof(Config.MenuClockFormat))]
    public string MenuClockFormat
    {
        get => Config.MenuClockFormat;
        set
        {
            if (Config.MenuClockFormat != value) Config.MenuClockFormat = value;
        }
    }

    [UIValue(nameof(ClockFormatOptions))]
    [UsedImplicitly]
    public List<object> ClockFormatOptions { get; } = [..PluginConfig.CLOCK_FORMATS];

    [UIValue(nameof(Config.MenuCounterHeight))]
    public float MenuCounterHeight
    {
        get => Config.MenuCounterHeight;
        set
        {
            if (Config.MenuCounterHeight != value) Config.MenuCounterHeight = value;
        }
    }

    [UIValue(nameof(Config.MenuCounterDistance))]
    public float MenuCounterDistance
    {
        get => Config.MenuCounterDistance;
        set
        {
            if (Config.MenuCounterDistance != value) Config.MenuCounterDistance = value;
        }
    }

    [UIValue(nameof(Config.MenuCounterTilt))]
    public float MenuCounterTilt
    {
        get => Config.MenuCounterTilt;
        set
        {
            if (Config.MenuCounterTilt != value) Config.MenuCounterTilt = value;
        }
    }

    #endregion

    #region Icon Tab

    private readonly IList<string> _iconNames = new List<string>();

    [UIComponent("icon-list")]
    private CustomListTableData _iconList = null!;

    [UIValue(nameof(IsIconListLoaded))]
    public bool IsIconListLoaded
    {
        get;
        private set
        {
            field = value;
            NotifyPropertyChanged();
        }
    }

    private async Task LoadIconList(bool refresh)
    {
        _logger.Info("Loading custom icons");

        IsIconListLoaded = false;
        _iconNames.Clear();
        var icons = await _iconManager.GetIconsWithSpriteAsync(refresh);
        var data = new List<CustomListTableData.CustomCellInfo>(icons.Count + 1)
        {
            new(Localization.Get("HRZONE_CONFIG_MENU_ICON_DEFAULT"), null, _iconManager.DefaultIcon)
        };
        _iconNames.Add("");
        foreach (var (filename, sprite) in icons)
        {
            data.Add(new CustomListTableData.CustomCellInfo(filename, null, sprite));
            _iconNames.Add(filename);
        }

        _iconList.Data = data;
        _iconList.TableView.ReloadData();

        _logger.Debug($"Loaded {data.Count} custom icon options");

        UpdateCustomIconSelection();

        IsIconListLoaded = true;
    }

    private void UpdateCustomIconSelection()
    {
        var selected = _iconNames.IndexOf(Config.CustomIcon);
        if (selected < 0)
        {
            _iconList.TableView.ClearSelection();
            _iconList.TableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
        }
        else
        {
            _iconList.TableView.SelectCellWithIdx(selected);
            _iconList.TableView.ScrollToCellWithIdx(selected, TableView.ScrollPositionType.Center, false);
        }
    }

    [UIAction(nameof(OnIconListCellSelect))]
    [UsedImplicitly]
    private void OnIconListCellSelect(TableView view, int index)
    {
        if (view != _iconList.TableView) return;
        var iconName = _iconNames[index];
        _logger.Debug($"Selected icon {index} ({iconName})");
        Config.CustomIcon = iconName;
    }

    [UIAction(nameof(RefreshIconOptions))]
    [UsedImplicitly]
    private void RefreshIconOptions()
    {
        _ = LoadIconList(true);
    }

    [UIAction(nameof(OpenIconsFolder))]
    [UsedImplicitly]
    private void OpenIconsFolder()
    {
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = _iconManager.IconDirPath,
            UseShellExecute = true,
            Verb = "open"
        };

        System.Diagnostics.Process.Start(psi);
    }

    #endregion

    #region HR Zone & Theme Tabs

    [UIValue(nameof(Config.ColorizeIcon))]
    public bool ColorizeIcon
    {
        get => Config.ColorizeIcon;
        set
        {
            if (Config.ColorizeIcon != value) Config.ColorizeIcon = value;
        }
    }

    [UIValue(nameof(Config.Colorize))]
    public bool Colorize
    {
        get => Config.Colorize;
        set
        {
            if (Config.Colorize != value) Config.Colorize = value;
        }
    }

    private bool _applyingTheme;

    [UIValue(nameof(Config.ColorTheme))]
    public string ColorTheme
    {
        get => Config.ColorTheme;
        set
        {
            if (Config.ColorTheme != value) Config.ColorTheme = value;
        }
    }

    [UIValue(nameof(ThemeOptions))]
    [UsedImplicitly]
    public List<object> ThemeOptions { get; } = [..ColorThemeUtils.DROPDOWN_KEYS];

    [UIAction(nameof(GetThemeDisplayName))]
    [UsedImplicitly]
    private string GetThemeDisplayName(string key) => key switch
    {
        ColorThemeUtils.SUNSET_KEY => Localization.Get("HRZONE_CONFIG_MENU_THEME_SUNSET"),
        ColorThemeUtils.PASTEL_KEY => Localization.Get("HRZONE_CONFIG_MENU_THEME_PASTEL"),
        ColorThemeUtils.BRIGHT_KEY => Localization.Get("HRZONE_CONFIG_MENU_THEME_BRIGHT"),
        ColorThemeUtils.USER_THEME_KEY => Localization.Get("HRZONE_CONFIG_MENU_THEME_USER"),
        _ => key,
    };

    private Color[] CurrentZoneColors() => [Config.Zone1Color, Config.Zone2Color, Config.Zone3Color, Config.Zone4Color, Config.Zone5Color];

    private void ApplyTheme(string key)
    {
        if (_applyingTheme) return;

        Color[] colors;
        if (key == ColorThemeUtils.USER_THEME_KEY)
            colors = [Config.UserThemeZone1Color, Config.UserThemeZone2Color, Config.UserThemeZone3Color, Config.UserThemeZone4Color, Config.UserThemeZone5Color];
        else if (!ColorThemeUtils.TryGetPreset(key, out colors))
            return;

        _applyingTheme = true;
        try
        {
            Config.Zone1Color = colors[0];
            Config.Zone2Color = colors[1];
            Config.Zone3Color = colors[2];
            Config.Zone4Color = colors[3];
            Config.Zone5Color = colors[4];
        }
        finally
        {
            _applyingTheme = false;
        }
    }

    // A manual zone edit (a color picker change) makes the current screen colors the User Theme slot;
    // captured at the call sites because PropertyChanged is dispatched asynchronously and a
    // reactive handler could see stale guard state.
    private void CaptureUserTheme()
    {
        Config.UserThemeZone1Color = Config.Zone1Color;
        Config.UserThemeZone2Color = Config.Zone2Color;
        Config.UserThemeZone3Color = Config.Zone3Color;
        Config.UserThemeZone4Color = Config.Zone4Color;
        Config.UserThemeZone5Color = Config.Zone5Color;

        if (Config.ColorTheme != ColorThemeUtils.USER_THEME_KEY)
            Config.ColorTheme = ColorThemeUtils.USER_THEME_KEY;
    }

    private void ReconcileThemeSelection()
    {
        if (!ColorThemeUtils.TryGetPreset(Config.ColorTheme, out var preset)) return;
        if (ColorThemeUtils.ZoneColorsMatch(CurrentZoneColors(), preset)) return;

        _logger.Debug($"Stored zone colors do not match the '{Config.ColorTheme}' theme; moving them to '{ColorThemeUtils.USER_THEME_KEY}'");
        CaptureUserTheme();
    }

    [UIValue(nameof(Config.MaxHR))]
    public int MaxHR
    {
        get => Config.MaxHR;
        set
        {
            if (Config.MaxHR != value) Config.MaxHR = value;
        }
    }

    [UIValue(nameof(Config.Zone1Color))]
    public Color Zone1Color
    {
        get => Config.Zone1Color;
        set
        {
            if (Config.Zone1Color == value) return;
            Config.Zone1Color = value;
            CaptureUserTheme();
        }
    }

    [UIValue(nameof(Config.Zone2Color))]
    public Color Zone2Color
    {
        get => Config.Zone2Color;
        set
        {
            if (Config.Zone2Color == value) return;
            Config.Zone2Color = value;
            CaptureUserTheme();
        }
    }

    [UIValue(nameof(Config.Zone3Color))]
    public Color Zone3Color
    {
        get => Config.Zone3Color;
        set
        {
            if (Config.Zone3Color == value) return;
            Config.Zone3Color = value;
            CaptureUserTheme();
        }
    }

    [UIValue(nameof(Config.Zone4Color))]
    public Color Zone4Color
    {
        get => Config.Zone4Color;
        set
        {
            if (Config.Zone4Color == value) return;
            Config.Zone4Color = value;
            CaptureUserTheme();
        }
    }

    [UIValue(nameof(Config.Zone5Color))]
    public Color Zone5Color
    {
        get => Config.Zone5Color;
        set
        {
            if (Config.Zone5Color == value) return;
            Config.Zone5Color = value;
            CaptureUserTheme();
        }
    }

    [UIValue(nameof(Zone1Text))]
    public string Zone1Text => BuildZoneLabel(1);

    [UIValue(nameof(Zone2Text))]
    public string Zone2Text => BuildZoneLabel(2);

    [UIValue(nameof(Zone3Text))]
    public string Zone3Text => BuildZoneLabel(3);

    [UIValue(nameof(Zone4Text))]
    public string Zone4Text => BuildZoneLabel(4);

    [UIValue(nameof(Zone5Text))]
    public string Zone5Text => BuildZoneLabel(5);

    private string BuildZoneLabel(int zone)
    {
        var maxHr = Config.MaxHR;
        if (zone == 5)
            return Localization.Instance.GetFormatOrKey("HRZONE_CONFIG_MENU_ZONE5_LABEL_FORMAT", maxHr * 9 / 10);
        // highest HR still inside the zone: the boundary maxHr*(zone+5)/10 belongs to the zone above
        return Localization.Instance.GetFormatOrKey("HRZONE_CONFIG_MENU_ZONE_LABEL_FORMAT", zone, maxHr * (zone + 5) / 10 - 1);
    }

    private void NotifyZoneLabels()
    {
        NotifyPropertyChanged(nameof(Zone1Text));
        NotifyPropertyChanged(nameof(Zone2Text));
        NotifyPropertyChanged(nameof(Zone3Text));
        NotifyPropertyChanged(nameof(Zone4Text));
        NotifyPropertyChanged(nameof(Zone5Text));
    }

    #endregion

    #region Preview Counters

    private readonly List<CounterView> _previewViews = [];

    private void SetupPreviewCounters(AssetBundleManager abm, DiContainer di)
    {
        _logger.Debug("Setting up preview counters");

        var serverCounter = SetupCounterPreview<HRZoneFeedPreview>(abm, di, "HRZone Preview Server Counter");
        _feedPreviewComponent = serverCounter?.gameObject.GetComponent<HRZoneFeedPreview>();
        FeedPreviewCounter = serverCounter ?? CreatePreviewFallback("HRZone Server Preview Fallback");
    }

    private Transform? SetupCounterPreview<T>(AssetBundleManager abm, DiContainer di, string objectName) where T : Component, ICounterPreview
    {
        _logger.Debug($"Setting up {objectName}");
        var view = abm.SetupCustomCounter();
        if (view == null)
        {
            _logger.Warn($"Failed to setup {objectName}");
            return null;
        }

        _previewViews.Add(view);

        var container = view.Container;
        container.SetParent(transform, false);
        Destroy(view.Canvas);
        container.gameObject.name = objectName;
        container.localScale = Vector3.one / 12;

        var go = container.gameObject;
        go.SetActive(false);
        di.InstantiateComponent<T>(go);
        var component = go.GetComponent<T>();
        if (component == null)
        {
            _logger.Warn($"Failed to add {typeof(T).Name} to {objectName}");
            return container;
        }

        component.Init(view);
        go.SetActive(true);

        return container;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        foreach (var view in _previewViews)
        {
            view.Dispose();
        }

        _previewViews.Clear();
    }

    private Transform CreatePreviewFallback(string objectName)
    {
        var text = BeatSaberUI.CreateText(transform as RectTransform, Localization.Get("HRZONE_CONFIG_MENU_PREVIEW_LOAD_FAILED"),
            new Vector2(0.5f, 0.5f));
        text.color = Color.red;
        text.gameObject.name = objectName;
        return text.transform;
    }

    #endregion

    #region Server Tab

    [UIValue(nameof(DataSourceOptions))]
    [UsedImplicitly]
    public List<object> DataSourceOptions => [.._dataSourceManager.DataSources.Keys];

    [UIValue(nameof(Config.DataSource))]
    public string DataSource
    {
        get => Config.DataSource;
        set
        {
            if (Config.DataSource != value) Config.DataSource = value;
        }
    }

    [UIValue(nameof(Config.FeedLink))]
    public string FeedLink
    {
        get => Config.FeedLink;
        set
        {
            if (Config.FeedLink != value) Config.FeedLink = value;
        }
    }

    [UIValue(nameof(IsWebRequestSource))]
    [UsedImplicitly]
    public bool IsWebRequestSource => _dataSourceManager.GetFromKey(Config.DataSource)?.DataSourceType == typeof(WebRequest);

    [UIValue(nameof(FeedPreviewCounter))]
    public Transform FeedPreviewCounter { get; private set; } = null!;

    [UIValue(nameof(FeedPreviewText))]
    public string FeedPreviewText
    {
        get;
        private set
        {
            field = value;
            NotifyPropertyChanged();
        }
    } = "";

    [UIAction(nameof(GetDataSourceDisplayName))]
    [UsedImplicitly]
    private string GetDataSourceDisplayName(string key)
    {
        var descriptor = _dataSourceManager.GetFromKey(key);
        return descriptor?.GetDisplayName() ?? key;
    }

    #endregion

    #region Feed Preview

    private void AttachFeedPreview()
    {
        _feed.StateChanged -= ApplyFeedState;
        _feed.StateChanged += ApplyFeedState;

        // the shared poller is already running: the snapshot is the current state, no own request
        if (!_feed.HasState)
        {
            FeedPreviewText = "";
            _feedPreviewComponent?.ShowNoValue();
            return;
        }

        ApplyFeedState(_feed.Status, _feed.CurrentHR);
    }

    private void ApplyFeedState(MenuFeedStatus status, int? hr)
    {
        switch (status)
        {
            case MenuFeedStatus.HasValue when hr.HasValue:
                FeedPreviewText = "";
                _feedPreviewComponent?.SetHeartRate(hr.Value);
                break;
            case MenuFeedStatus.Disabled:
                _feedPreviewComponent?.ShowNoValue();
                FeedPreviewText = Localization.Get("HRZONE_DATA_SOURCE_MENU_FEED_DISABLED");
                break;
            case MenuFeedStatus.Failed:
                _feedPreviewComponent?.ShowNoValue();
                FeedPreviewText = $"<color=red>{Localization.Get("HRZONE_DATA_SOURCE_MENU_FEED_NO_CONNECTION")}</color>";
                break;
            default:
                _feedPreviewComponent?.ShowNoValue();
                FeedPreviewText = Localization.Get("HRZONE_DATA_SOURCE_MENU_FEED_NOT_SET");
                break;
        }
    }

    #endregion
}
