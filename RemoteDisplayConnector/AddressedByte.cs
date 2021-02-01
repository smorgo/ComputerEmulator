namespace RemoteDisplayConnector
{
    public class AddressedByte
    {
        public ushort Address {get; private set;}
        public byte Value {get; private set;}
        public AddressedByte(ushort address, byte value)
        {
            Address = address;
            Value = value;
        }
    }
}