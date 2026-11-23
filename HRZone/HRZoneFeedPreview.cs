using System.ComponentModel;
using HRZone.Configuration;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using IPALogger = IPA.Logging.Logger;

namespace HRZone;

internal class HRZoneFeedPreview : MonoBehaviour, ICounterPreview
{
    [Inject]
    private readonly IPALogger _logger = null!;

    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly IconManager _iconManager = null!;

    private CounterView _view = null!;

    private int? _lastHr;

    [UsedImplicitly]
    public void Init(CounterView view)
    {
        _logger.Trace("Init");
        _view = view;
        _view.ApplyBloom(true); // previews are composited outside the game's bloom pass: always flat
        ShowNoValue();
    }

    private void OnEnable()
    {
        _logger.Trace("OnEnable");
        OnConfigChanged(_config, new PropertyChangedEventArgs(""));
        _config.PropertyChanged += OnConfigChanged;
        _iconManager.IconsLoaded += UpdateIcon;
    }

    private void OnDisable()
    {
        _logger.Trace("OnDisable");
        _config.PropertyChanged -= OnConfigChanged;
        _iconManager.IconsLoaded -= UpdateIcon;
    }

    public void SetHeartRate(int hr)
    {
        _lastHr = hr;
        _view.ApplyHeartRate(hr);
    }

    public void ShowNoValue()
    {
        _lastHr = null;
        _view.ApplyNoValue();
    }

    private void OnConfigChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(args.PropertyName) || args.PropertyName == nameof(_config.ColorizeIcon) || args.PropertyName == nameof(_config.Colorize))
        {
            if (_lastHr.HasValue) _view.ApplyHeartRate(_lastHr.Value);
            else _view.ApplyNoValue();
        }

        if (string.IsNullOrEmpty(args.PropertyName) || args.PropertyName == nameof(_config.CustomIcon))
        {
            UpdateIcon();
        }
    }

    private void UpdateIcon()
    {
        _view.ApplyIcon(_config.CustomIcon);
    }
}
