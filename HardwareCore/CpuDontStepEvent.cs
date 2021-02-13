namespace HardwareCore
{
    public class CpuDontStepEvent : ICpuStepEvent
    {
        public static CpuDontStepEvent GetInstance()
        {
            return new CpuDontStepEvent();
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