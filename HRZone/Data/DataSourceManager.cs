using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HRZone.Configuration;
using HRZone.Data.DataSources;
using HRZone.Data.SourceDescriptors;
using HRZone.Utils;
using JetBrains.Annotations;
using Zenject;
using Component = UnityEngine.Component;
using IPALogger = IPA.Logging.Logger;

namespace HRZone.Data;

internal sealed class DataSourceManager : IDisposable
{
    private const string WEBREQUEST_KEY = "WebRequest";

    [Inject]
    private readonly IPALogger _logger = null!;

    [Inject]
    private readonly PluginConfig _config = null!;

    [Inject]
    private readonly DiContainer _diContainer = null!;

    private readonly Dictionary<string, IDataSourceDescriptor> _sources = new(StringComparer.InvariantCultureIgnoreCase);

    internal IReadOnlyDictionary<string, IDataSourceDescriptor> DataSources => _sources;

    [Inject]
    [UsedImplicitly]
    private void Init()
    {
        RegisterInternalDataSources();
    }

    void IDisposable.Dispose()
    {
        foreach (var pair in _sources)
        {
            // Component based descriptor will have the GameObject destroyed by Zenject
            try
            {
                if (pair.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to dispose data source descriptor for {pair.Value.Key}: {e}");
                _logger.Error(e);
            }
        }

        _sources.Clear();
    }

    internal IDataSourceDescriptor? GetFromKey(string str) => _sources.GetValueOrDefault(str);

    public void RegisterDataSource<T>(string key, Func<CancellationToken, Task<string>> getStatusText,
        Func<bool> precondition, string? name = null, string? nameKey = null) where T : class, IHRDataSource
    {
        var descriptor = new GenericSourceDescriptor<T>(key, getStatusText, precondition, name, nameKey);
        RegisterDataSource(descriptor);
    }

    public void RegisterDataSource<T>(string key, Func<string> getStatusText, Func<bool> precondition,
        string? name = null, string? nameKey = null) where T : class, IHRDataSource
    {
        RegisterDataSource<T>(key, _ => Task.FromResult(getStatusText()), precondition, name, nameKey);
    }

    public void RegisterDataSource<T>(IDataSourceDescriptor<T> descriptor) where T : class, IHRDataSource
    {
        var key = descriptor.Key;
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key cannot be null or whitespace!", nameof(key));
        if (_sources.ContainsKey(key)) throw new ArgumentException($"Key {key} already exists!", nameof(key));
        _logger.Debug("Registering data source: " + key);
        _sources.Add(descriptor.Key, descriptor);
    }

    public void RegisterDataSource<TDesc, TSource>() where TDesc : class, IDataSourceDescriptor<TSource> where TSource : class, IHRDataSource
    {
        TDesc source;
        if (typeof(Component).IsAssignableFrom(typeof(TDesc)))
        {
            _logger.Spam($"Instantiating {typeof(TDesc).Name} on new game object");
            var instance = _diContainer.InstantiateComponent(typeof(TDesc), _diContainer.CreateEmptyGameObject(typeof(TDesc).Name));
            // ReSharper disable once SuspiciousTypeConversion.Global
            source = (TDesc)(object)instance;
            instance.gameObject.name = $"HRZone Data Source Descriptor - {source.Key}";
        }
        else
        {
            _logger.Spam($"Instantiating {typeof(TDesc).Name} as normal object");
            source = _diContainer.Instantiate<TDesc>();
        }

        RegisterDataSource(source);
    }

    #region Internal Data Sources

    private void RegisterInternalDataSources()
    {
        _logger.Debug("Registering internal data sources");
        RegisterDataSource(new SimpleSourceDescriptor<WebRequest>(WEBREQUEST_KEY, "HRZONE_WEBREQUEST_SOURCE_DESCRIPTOR_LABEL",
            () => _config.FeedLink, _config, nameof(_config.FeedLink), nameKey: "HRZONE_WEBREQUEST_SOURCE_DESCRIPTOR_NAME"));
    }

    #endregion
}
