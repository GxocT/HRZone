using System;
using System.Collections.Generic;
using HRZone.Configuration;
using HRZone.Data;
using HRZone.Data.DataSources.Base;
using HRZone.UI;
using HRZone.Utils;
using IPA.Loader;
using IPA.Logging;
using Zenject;

namespace HRZone.Installers;

public class AppInstaller : Installer<AppInstaller>
{
    private readonly PluginConfig _config;
    private readonly Logger _logger;
    private readonly PluginMetadata _pluginMetadata;

    private readonly Dictionary<string, Logger> _loggers = new();

    private AppInstaller(PluginConfig config, Logger logger, PluginMetadata pluginMetadata)
    {
        _config = config;
        _logger = logger;
        _pluginMetadata = pluginMetadata;
    }

    public override void InstallBindings()
    {
        Container.Bind<Logger>().FromMethod(CreateChildLogger).AsTransient().When(ShouldBindLogger);
        Container.Bind<Logger>().WithId(typeof(BaseConfigViewController)).FromMethod(CreateChildLogger).AsTransient();
        Container.Bind<Logger>().WithId(typeof(DataSource)).FromMethod(CreateChildLogger).AsTransient();
        Container.Bind<Logger>().WithId(typeof(HRZoneCounter)).FromMethod(CreateChildLogger).AsTransient();

        Container.BindInstance(_config).AsSingle();
        Container.BindInterfacesAndSelfTo<AssetBundleManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<DataSourceManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<IconManager>().AsSingle();
    }

    private Logger CreateChildLogger(InjectContext context)
    {
        var type = context.Identifier as Type ?? context.ObjectType;
        var name = type.Name;
        if (_loggers.TryGetValue(name, out var logger))
        {
            _logger.Spam($"Using cached child logger for {name}");
            return logger;
        }

        _logger.Spam($"Creating child logger for {name}");
        logger = Plugin.GetChildLogger(name);
        _loggers[name] = logger;
        return logger;
    }

    private bool ShouldBindLogger(InjectContext context) => context.ObjectType.Assembly == _pluginMetadata.Assembly;
}
