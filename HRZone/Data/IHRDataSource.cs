using System;

namespace HRZone.Data;

internal interface IHRDataSource
{
    event EventHandler<HRDataReceivedEventArgs>? OnHRDataReceived;
}
