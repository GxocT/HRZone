using System;

namespace HRZone.Data;

internal interface IInGameHRProvider
{
    event Action<int?>? HRChanged;

    /// <summary>
    ///     Null until the first accepted heart rate value of the session arrives.
    /// </summary>
    int? CurrentHR { get; }
}
