namespace HardwareCore
{
    public class CpuDontStepEvent : CpuStepEvent
    {
        public override void WaitOne()
        {
            // Do nothing
        }
    }
}