using System.Collections;
using System.ComponentModel;
using HRZone.Configuration;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;
using IPALogger = IPA.Logging.Logger;

namespace HRZone;

internal class HRZonePreview : MonoBehaviour, ICounterPreview
{
    [Inject]
    private readonly IPALogger _logger = null!;

    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly IconManager _iconManager = null!;

    private readonly WaitForSeconds _delay = new(0.05f);

    private CounterView _view = null!;

    private Coroutine? _coroutine;

    private const int PREVIEW_MIN = 60;

    private int _value;

    private int _direction = 1;

    [UsedImplicitly]
    public void Init(CounterView view)
    {
        _logger.Trace("Init");
        _view = view;
        _view.ApplyBloom(true); // previews are composited outside the game's bloom pass: always flat
        _value = PREVIEW_MIN;
    }

    private void OnEnable()
    {
        _logger.Trace("OnEnable");
        _coroutine = StartCoroutine(UpdateText());
        OnConfigChanged(_config, new PropertyChangedEventArgs(""));
        _config.PropertyChanged += OnConfigChanged;
        _iconManager.IconsLoaded += UpdateIcon;
    }

    private void OnDisable()
    {
        _logger.Trace("OnDisable");
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }

        _config.PropertyChanged -= OnConfigChanged;
        _iconManager.IconsLoaded -= UpdateIcon;
    }

    private IEnumerator UpdateText()
    {
        while (enabled)
        {
            var hrMax = _config.MaxHR;
            if (_value < PREVIEW_MIN) _value = PREVIEW_MIN;
            else if (_value > hrMax) _value = hrMax;

            _view.ApplyHeartRate(_value);

            if (_direction > 0)
            {
                if (_value >= hrMax)
                {
                    _direction = -1;
                    _value--;
                }
                else
                {
                    _value++;
                }
            }
            else
            {
                if (_value <= PREVIEW_MIN)
                {
                    _direction = 1;
                    _value++;
                }
                else
                {
                    _value--;
                }
            }

            yield return _delay;
        }
    }

    private void OnConfigChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (string.IsNullOrEmpty(args.PropertyName) || args.PropertyName == nameof(_config.ColorizeIcon) || args.PropertyName == nameof(_config.Colorize))
        {
            _view.ApplyHeartRate(_value);
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
