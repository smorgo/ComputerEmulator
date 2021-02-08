using System.Threading;

namespace HardwareCore
{
    public class CpuStepEvent
    {
        private ManualResetEvent _event => new ManualResetEvent(false);

        public void Set()
        {
            _event.Set();
        }
        public void Reset()
        {
            _event.Reset();
        }
        public void WaitOne()
        {
            _event.WaitOne();
        }
    }
}