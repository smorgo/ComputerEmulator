using System;
using System.Diagnostics;
using System.Threading;

namespace HardwareCore
{

    public class CpuHoldEvent : ICpuHoldEvent
    {
        public ManualResetEventSlim _event = new ManualResetEventSlim(false);

        public virtual void Set()
        {
            _event.Set();
        }
        public virtual void Reset()
        {
            _event.Reset();
        }
        public virtual void WaitOne()
        {
            _event.Wait();
        }
    }
}