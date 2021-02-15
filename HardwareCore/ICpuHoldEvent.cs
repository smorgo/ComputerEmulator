using System;

namespace HardwareCore
{
    public interface ICpuHoldEvent
    {
        void Reset();
        void Set();
        bool WaitOne(TimeSpan maxDuration);
    }
}