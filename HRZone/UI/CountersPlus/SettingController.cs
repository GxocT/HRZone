using BeatSaberMarkupLanguage.Attributes;
using HRZone.Configuration;
using UnityEngine;
using Zenject;

namespace HRZone.UI.CountersPlus;

// The host lives in Counters+'s Menu container (bound WithId by their MenuUIInstaller),
// so it is a plain MonoBehaviour, not our BaseConfigViewController.
internal class SettingController : MonoBehaviour
{
    [Inject]
    private readonly PluginConfig _config = null!;

    [UIValue(nameof(PluginConfig.CountersPlusOffsetY))]
    public float CountersPlusOffsetY
    {
        get => _config.CountersPlusOffsetY;
        set
        {
            if (_config.CountersPlusOffsetY != value) _config.CountersPlusOffsetY = value;
        }
    }
}
