using System;
using JetBrains.Annotations;
using Zenject;

namespace HRZone.Data;

internal class LiveHRProvider : IInGameHRProvider, IDisposable
{
    [InjectOptional]
    private readonly HRDataManager? _hrDataManager = null;

    private int? _currentHR;

    public int? CurrentHR => _currentHR;

    public event Action<int?>? HRChanged;

    [Inject]
    [UsedImplicitly]
    private void Init()
    {
        if (_hrDataManager == null) return;
        _hrDataManager.OnHRUpdate += OnHRUpdated;
        _currentHR = _hrDataManager.CurrentHR;
    }

    void IDisposable.Dispose()
    {
        if (_hrDataManager == null) return;
        _hrDataManager.OnHRUpdate -= OnHRUpdated;
    }

    private void OnHRUpdated(int? hr)
    {
        if (hr == _currentHR) return;
        _currentHR = hr;
        var handler = HRChanged;
        handler?.Invoke(hr);
    }
}
