namespace Debugger
{
    public class MemoryChangedEventArgs 
    {
        public ushort Address {get; private set;}
        public byte Value {get; private set;}

        public MemoryChangedEventArgs(ushort address, byte value)
        {
            Address = address;
            Value = value;
        }
    }
}
