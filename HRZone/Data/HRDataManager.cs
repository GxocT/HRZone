using System;
using IPA.Logging;
using IPA.Utilities.Async;
using Zenject;

namespace HRZone.Data;

internal class HRDataManager : IInitializable, IDisposable
{
    [Inject]
    private readonly Logger _logger = null!;

    [InjectOptional]
    private readonly IHRDataSource? _dataSource = null;

    /// <summary>
    ///     Always invoked on the main thread when new HR data is received.
    /// </summary>
    public event Action<int?>? OnHRUpdate;

    private int? _currentHr;

    public int? CurrentHR => _currentHr;

    public void Initialize()
    {
        _logger.Debug("HRDataManager Init");
        if (_dataSource == null)
        {
            _logger.Critical("Data source is null!");
            return;
        }

        _dataSource.OnHRDataReceived += OnHrDataReceivedInternalHandler;
    }

    public void Dispose()
    {
        _logger.Debug("HRDataManager Dispose");
        if (_dataSource != null)
        {
            _dataSource.OnHRDataReceived -= OnHrDataReceivedInternalHandler;
        }
    }

    private void OnHrDataReceivedInternalHandler(object sender, HRDataReceivedEventArgs args)
    {
        _currentHr = args.HR;

        UnityMainThreadTaskScheduler.Factory.StartNew(() =>
        {
            try
            {
                var handler = OnHRUpdate;
                handler?.Invoke(args.HR);
            }
            catch (Exception e)
            {
                _logger.Critical($"Exception Caught while broadcasting hr update event: {e.Message}");
                _logger.Critical(e);
            }
        });
    }
}
