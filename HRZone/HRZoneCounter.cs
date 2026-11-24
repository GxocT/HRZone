using System.ComponentModel;
using HRZone.Configuration;
using HRZone.Data;
using UnityEngine;
using Zenject;
using Logger = IPA.Logging.Logger;

namespace HRZone;

internal abstract class HRZoneCounter
{
    [Inject(Id = typeof(HRZoneCounter))]
    private readonly Logger _logger = null!;

    [Inject]
    protected readonly PluginConfig Config = null!;

    [Inject]
    private readonly AssetBundleManager _assetBundleManager = null!;

    [InjectOptional]
    protected readonly IInGameHRProvider? _hrProvider = null;

    [InjectOptional]
    private readonly CoreGameHUDController.InitData? _hudInitData = null;

    private CounterView? _view;

    protected bool Setup()
    {
        if (_hrProvider == null)
        {
            return false;
        }

        if (_hudInitData is { hide: true })
        {
            _logger.Info("CoreGameHUDController.InitData says to hide the HUD, HRZone hides.");
            return false;
        }

        if (Utils.Utils.IsInReplay())
        {
            _logger.Info("We are in a replay, HRZone hides.");
            return false;
        }

        var view = _assetBundleManager.SetupCustomCounter();
        if (view == null)
        {
            _logger.Warn("No Counter asset is loaded!");
            return false;
        }

        if (!SetupCounter(view))
        {
            _logger.Warn("Failed to set up the counter, destroying counter");
            Object.Destroy(view.Canvas);
            view.Dispose();
            return false;
        }

        _view = view;
        if (ShouldStartHidden() && _hrProvider.CurrentHR == null)
        {
            view.Canvas.SetActive(false);
        }

        OnHRUpdate(_hrProvider.CurrentHR); // give it an initial value

        _hrProvider.HRChanged += OnHRUpdate;
        Config.PropertyChanged += OnSettingChange;
        _logger.Info("HRZone created");
        return true;
    }

    protected abstract bool SetupCounter(CounterView view);

    // Auto-hide hook: a subclass that starts its canvas hidden when the feed has no value
    // yet. The base reveals it on the first real value and never hides it again.
    protected virtual bool ShouldStartHidden() => false;

    private void OnHRUpdate(int? hr)
    {
        if (hr.HasValue)
        {
            _view?.Canvas.SetActive(true);
            _view?.ApplyHeartRate(hr.Value);
        }
        else
        {
            _view?.ApplyNoValue();
        }
    }

    private void OnSettingChange(object? sender, PropertyChangedEventArgs args)
    {
        // Tinting is applied inside the view, so a toggle needs an immediate re-render of the current state.
        if (string.IsNullOrEmpty(args.PropertyName)
            || args.PropertyName == nameof(PluginConfig.ColorizeIcon)
            || args.PropertyName == nameof(PluginConfig.Colorize))
        {
            OnHRUpdate(_hrProvider?.CurrentHR);
        }

        OnSettingChange(args.PropertyName);
    }

    protected virtual void OnSettingChange(string propertyName)
    {
    }

    protected void Teardown()
    {
        if (_hrProvider != null)
        {
            _hrProvider.HRChanged -= OnHRUpdate;
        }

        Config.PropertyChanged -= OnSettingChange;

        _view?.Dispose();
        _view = null;
    }
}
