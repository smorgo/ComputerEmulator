namespace HardwareCore
{
    public class MockCpuHoldEvent : MockEventBase, ICpuHoldEvent
    {
        public MockCpuHoldEvent() : base(true)
        {
        }
    }
}