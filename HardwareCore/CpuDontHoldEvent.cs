namespace HardwareCore
{
    public class CpuDontHoldEvent : ICpuHoldEvent
    {
        public static CpuDontHoldEvent GetInstance()
        {
            return new CpuDontHoldEvent();
        }

        public void Reset()
        {
        }

        public void Set()
        {
        }

        public void WaitOne()
        {
        }
    }
}