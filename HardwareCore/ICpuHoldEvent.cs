namespace HardwareCore
{
    public interface ICpuHoldEvent
    {
        void Reset();
        void Set();
        void WaitOne();
    }
}