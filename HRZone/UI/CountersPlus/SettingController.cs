using System.ComponentModel;
using BeatSaberMarkupLanguage.Attributes;
using BGLib.Polyglot;
using HRZone.Configuration;
using HRZone.Data;
using HRZone.Utils;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using Zenject;

namespace HRZone.UI.CountersPlus;

// The host lives in Counters+'s Menu container (bound WithId by their MenuUIInstaller),
// so it is a plain MonoBehaviour, not our BaseConfigViewController.
internal class SettingController : MonoBehaviour
{
    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly DataSourceManager _dataSourceManager = null!;

    [UIComponent("data-source-text")]
    private TMP_Text _dataSourceText = null!;

    [UIComponent("integration-text")]
    private TMP_Text _integrationText = null!;

    private bool _parsed;

    private void OnEnable()
    {
        _config.PropertyChanged += OnConfigChanged;
        UpdateTexts();
    }

    private void OnDisable()
    {
        _config.PropertyChanged -= OnConfigChanged;
    }

    [UIAction("#post-parse")]
    [UsedImplicitly]
    private void PostParse()
    {
        _parsed = true;
        UpdateTexts();
    }

    private void OnConfigChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(args.PropertyName)
            || args.PropertyName == nameof(PluginConfig.DataSource)
            || args.PropertyName == nameof(PluginConfig.CountersPlusIntegration))
        {
            UpdateTexts();
        }
    }

    private void UpdateTexts()
    {
        if (!_parsed) return;

        var descriptor = _dataSourceManager.GetFromKey(_config.DataSource);
        _dataSourceText.text = descriptor == null
            ? Localization.Get("HRZONE_COUNTERS_PLUS_MENU_DATA_SOURCE_UNKNOWN")
            : Localization.Instance.GetFormatOrKey("HRZONE_COUNTERS_PLUS_MENU_DATA_SOURCE", descriptor.GetDisplayName());

        _integrationText.text = Localization.Get(_config.CountersPlusIntegration
            ? "HRZONE_COUNTERS_PLUS_MENU_INTEGRATION_ON"
            : "HRZONE_COUNTERS_PLUS_MENU_INTEGRATION_OFF");
    }
}
