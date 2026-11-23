using System;

namespace HRZone.Data;

internal class HRDataReceivedEventArgs : EventArgs
{
    public int HR { get; private set; }

    public HRDataReceivedEventArgs(int hr)
    {
        HR = hr;
    }
}
