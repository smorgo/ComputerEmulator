namespace HardwareCore
{
    public class CpuDontHoldEvent : CpuHoldEvent
    {
        public override void WaitOne()
        {
            // Do nothing
        }
    }
}