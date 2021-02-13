using System;
using System.Diagnostics;
using System.Threading;

namespace HardwareCore
{
    public class CpuStepEvent
    {
        private static CpuStepEvent _instance;
        private bool _set = true;

        public CpuStepEvent()
        {
            if(_instance != null)
            {
                throw new InvalidOperationException("Multiple instances of the CpuStepEvent have been created");
            }
            _instance = this;
        }
        public virtual void Set()
        {
            _set = true;
        }
        public virtual void Reset()
        {
            _set = false;
        }
        public virtual void WaitOne()
        {
            while(!_set)
            {
                Thread.Sleep(1);
            }
        }
    }
}