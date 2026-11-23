using HRZone.Configuration;
using HRZone.Installers;
using HRZone.Patches;
using IPA;
using IPA.Loader;
using IPA.Logging;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace HRZone;

[Plugin(RuntimeOptions.SingleStartInit)]
[NoEnableDisable]
public class Plugin
{
    internal static Plugin Instance { get; private set; } = null!;
    internal static IPALogger Logger { get; private set; } = null!;
    internal static PluginConfig Config { get; private set; } = null!;

    internal PluginMetadata? ScoreSaberMeta { get; }
    internal PluginMetadata? BeatLeaderMeta { get; }

    private const string ScoreSaberId = "ScoreSaber";
    private const string BeatLeaderId = "BeatLeader";

    [Init]
    public Plugin(IPALogger logger, IPA.Config.Config conf, PluginMetadata metadata, Zenjector zenject)
    {
        Instance = this;
        Logger = logger;
        var config = PluginConfig.Initialize(GetChildLogger(nameof(PluginConfig)), conf);
        Config = config;

        ScoreSaberMeta = Utils.Utils.FindEnabledPluginMetadata(ScoreSaberId);
        BeatLeaderMeta = Utils.Utils.FindEnabledPluginMetadata(BeatLeaderId);

        zenject.Install<AppInstaller>(Location.App, config, GetChildLogger(nameof(AppInstaller)), metadata);
        zenject.Install<MenuInstaller>(Location.Menu);
        zenject.Install<GameplayHeartRateInstaller>(Location.Player);
        zenject.Install<GameInstaller>(Location.Player);

        zenject.Expose<FlyingGameHUDRotation>("Environment");

        LocalizationPatch.Patch();

        Logger.Info("HRZone initialized.");
    }

    internal static IPALogger GetChildLogger(string name) => Logger.GetChildLogger(name);
}
