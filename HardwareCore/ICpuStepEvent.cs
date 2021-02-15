using System;
using System.Threading;

namespace HardwareCore
{
    public interface ICpuStepEvent
    {
        void Reset();
        void Set();
        bool WaitOne(TimeSpan maxDuration);
    }
}