using System;
using System.Reflection;
using HRZone.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Logger = IPA.Logging.Logger;
using Object = UnityEngine.Object;

namespace HRZone;

internal class AssetBundleManager : IInitializable, IDisposable
{
    private GameObject? CounterPrefab { get; set; }

    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly Logger _logger = null!;

    [Inject]
    private readonly IconManager _iconManager = null!;

    public void Initialize()
    {
        using var resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("HRZone.Resources.hrzone");
        if (resourceStream == null)
        {
            _logger.Error("Failed to find hrzone AssetBundle from ManifestResourceStream!");
            return;
        }

        try
        {
            // Load the AssetBundle
            var bundle = AssetBundle.LoadFromStream(resourceStream);
            CounterPrefab = bundle.LoadAsset<GameObject>("Assets/HRZone.prefab");
            bundle.Unload(false);
            if (CounterPrefab == null)
            {
                throw new NullReferenceException("The counter prefab is null!");
            }

            _logger.Info("AssetBundle Loaded!");
        }
        catch (Exception e)
        {
            _logger.Error($"Cannot load the counter prefab from the asset bundle: {e.Message}");
            _logger.Error(e);
        }
    }

    public void Dispose()
    {
        if (CounterPrefab != null)
        {
            Object.Destroy(CounterPrefab);
            CounterPrefab = null;
        }
    }

    internal CounterView? SetupCustomCounter()
    {
        _logger.Info("Creating HRZone object from prefab");
        if (CounterPrefab == null)
        {
            _logger.Warn("Counter prefab is not loaded!");
            return null;
        }

        var canvas = Object.Instantiate(CounterPrefab);
        if (canvas == null)
        {
            _logger.Warn("Failed to instantiate counter prefab");
            return null;
        }

        var container = canvas.transform.GetChild(0)!;
        var icon = container.GetChild(0).gameObject;
        var replayIcon = icon.transform.GetChild(0).gameObject;
        var numbers = container.GetChild(1).GetComponent<TMP_Text>();

        var iconImage = icon.GetComponent<Image>();

        replayIcon.SetActive(false);

        var view = new CounterView(canvas, container, numbers, iconImage, _iconManager, _config);
        view.ApplyBloom(_config.NoBloom);
        view.ApplyIcon(_config.CustomIcon);
        return view;
    }
}
