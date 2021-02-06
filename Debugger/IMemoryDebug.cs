using System;

namespace Debugger
{
    public interface IMemoryDebug
    {
        public byte Read(ushort address);
        public ushort ReadWord(ushort address);
        public byte[] ReadBlock(ushort startAddress, ushort endAddress);
        public class Change 
        {
            public ushort Address {get; private set;}
            public byte Value {get; private set;}

            public Change(ushort address, byte value)
            {
                Address = address;
                Value = value;
            }
        }

        EventHandler<Change> MemoryChanged {get;set;}
    }
}
