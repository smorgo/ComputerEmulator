using System;

namespace Debugger
{

    public interface IMemoryDebug
    {
        public byte Read(ushort address);
        public ushort ReadWord(ushort address);
        public byte[] ReadBlock(ushort startAddress, ushort endAddress);
        public void Write(ushort address, byte value);
        public void WriteWord(ushort address, ushort value);
        EventHandler<MemoryChangedEventArgs> MemoryChanged {get;set;}
    }
}
