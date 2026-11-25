using CountersPlus.Counters.Interfaces;
using CountersPlus.Custom;
using CountersPlus.Utils;
using UnityEngine;
using Zenject;
using Logger = IPA.Logging.Logger;

namespace HRZone;

internal sealed class HRZoneCountersPlus : HRZoneCounter, ICounter
{
    // The Counters+ canvas is itself scaled 1/Size (default Size 10), unlike the standalone's own world-space canvas,
    // so the container needs its own baseline; the Menu-tab constant does not apply here.
    private const float CONTAINER_SCALE = 1f / 30f;

    [Inject]
    private readonly Logger _logger = null!;

    [Inject]
    private readonly CanvasUtility _canvasUtility = null!;

    [Inject]
    private readonly CustomConfigModel _settings = null!;

    void ICounter.CounterInit()
    {
        if (!Config.ModEnable || !Config.CountersPlusIntegration)
        {
            return;
        }

        _logger.Info("Initializing Counters+ counter");
        Setup();
    }

    void ICounter.CounterDestroy()
    {
        Teardown();
    }

    protected override bool ShouldStartHidden() => Config.AutoHideCounter && _hrProvider?.CurrentHR == null;

    protected override GameObject GetVisibilityRoot(CounterView view) => view.Container.gameObject;

    protected override bool SetupCounter(CounterView view)
    {
        var canvas = _canvasUtility.GetCanvasFromID(_settings.CanvasID);
        var canvasSettings = _canvasUtility.GetCanvasSettingsFromID(_settings.CanvasID);
        if (canvas == null || canvasSettings == null)
        {
            _logger.Warn("Cannot find Counters+ canvas");
            return false;
        }

        var anchoredPosition = (Vector2)(_canvasUtility.GetAnchoredPositionFromConfig(_settings) * canvasSettings.PositionScale)
                               + Vector2.up * Config.CountersPlusOffsetY * canvasSettings.Size;

        var container = view.Container;
        container.gameObject.name = "HRZone Counters+";
        container.localScale = Vector3.one * CONTAINER_SCALE;
        container.SetParent(canvas.transform, false);
        ((RectTransform)container).anchoredPosition = anchoredPosition;

        Object.Destroy(view.Canvas);
        return true;
    }
}
