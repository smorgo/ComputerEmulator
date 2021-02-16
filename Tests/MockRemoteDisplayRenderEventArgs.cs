namespace Tests
{
    public class MockRemoteDisplayRenderEventArgs
    {
        public ushort Address {get; private set;}
        public byte Value {get; private set;}
        public MockRemoteDisplayRenderEventArgs(ushort address, byte value)
        {
            Address = address;
            Value = value;
        }
    }
}