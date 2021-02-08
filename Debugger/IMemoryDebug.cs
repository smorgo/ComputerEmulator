using System;

namespace Debugger
{

    public interface IMemoryDebug
    {
        public byte Read(ushort address);
        public ushort ReadWord(ushort address);
        public byte[] ReadBlock(ushort startAddress, ushort endAddress);

        EventHandler<MemoryChangedEventArgs> MemoryChanged {get;set;}
    }
}
