using System;
using IPA.Logging;
using Zenject;

namespace HRZone.Data.DataSources.Base;

internal abstract class DataSource : IHRDataSource, IInitializable, IDisposable
{
    [Inject(Id = typeof(DataSource))]
    private readonly Logger _logger = null!;

    public event EventHandler<HRDataReceivedEventArgs>? OnHRDataReceived;

    protected void OnHeartRateDataReceived(int hr)
    {
        try
        {
            var handler = OnHRDataReceived;
            handler?.Invoke(this, new HRDataReceivedEventArgs(hr));
        }
        catch (Exception e)
        {
            Plugin.Logger.Critical($"Exception Caught while broadcasting hr update event: {e.Message}");
            Plugin.Logger.Critical(e);
        }
    }

    public virtual void Initialize()
    {
        try
        {
            Start();
            _logger.Info("Start updating heart rate");
        }
        catch (Exception e)
        {
            _logger.Error($"Could not start bpm downloader. {e.Message}");
            _logger.Critical(e);
            Stop();
        }
    }

    public virtual void Dispose()
    {
        Stop();
    }

    protected abstract void Start();

    protected abstract void Stop();
}
