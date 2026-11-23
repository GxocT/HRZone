using HRZone.Configuration;
using HRZone.Data;
using IPA.Logging;
using Zenject;

namespace HRZone.Installers;

public class GameInstaller : Installer<GameInstaller>
{
    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly Logger _logger = null!;

    public override void InstallBindings()
    {
        if (!_config.ModEnable)
        {
            return;
        }

        _logger.Debug("Binding live HR provider");
        Container.BindInterfacesAndSelfTo<LiveHRProvider>().AsSingle();

        _logger.Debug("Binding HR Counter");
        Container.BindInterfacesTo<HRZoneStandalone>().AsSingle().NonLazy();
    }
}
