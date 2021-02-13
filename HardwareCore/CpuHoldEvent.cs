using System;
using System.Diagnostics;
using System.Threading;

namespace HardwareCore
{
    public class CpuHoldEvent
    {
        private static CpuHoldEvent _instance;
        private bool _set;
        //public static ManualResetEventSlim _event => new ManualResetEventSlim(false);

        public CpuHoldEvent()
        {
            if(_instance != null)
            {
                throw new InvalidOperationException("Multiple instances of the CpuHoldEvent have been created");
            }
            _instance = this;
        }
        public virtual void Set()
        {
            // _event.Set();
            _set = true;
        }
        public virtual void Reset()
        {
            // _event.Reset();
            _set = false;
        }
        public virtual void WaitOne()
        {
            // _event.Wait();
            while(!_set)
            {
                Thread.Sleep(1);
            }
        }
    }
}