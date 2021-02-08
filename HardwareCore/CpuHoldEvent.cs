using System.Threading;

namespace HardwareCore
{
    public class CpuHoldEvent
    {
        private ManualResetEvent _event => new ManualResetEvent(true);

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