namespace HardwareCore
{
    public interface ICpuStepEvent
    {
        void Reset();
        void Set();
        void WaitOne();
    }
}