using System;
using Debugger;

namespace Tests
{
    public class MockMemoryDebug : IMemoryDebug
    {
        public EventHandler<MemoryChangedEventArgs> MemoryChanged { get; set; }

        public byte Read(ushort address)
        {
            return (byte)address;
        }

        public byte[] ReadBlock(ushort startAddress, ushort endAddress)
        {
            var start = Math.Min(startAddress, endAddress);
            var end = Math.Max(startAddress, endAddress);
            var size = endAddress - startAddress + 1;
            var buffer = new byte[size];
            for(var ix = 0; ix < size; ix++)
            {
                buffer[ix] = Read((ushort)(startAddress + ix));
            }
            return buffer;
        }

        public ushort ReadWord(ushort address)
        {
            var lsb = Read(address);
            var msb = Read((ushort)(address+1));
            return (ushort)((msb << 8) + lsb);
        }

        public void Write(ushort address, byte value)
        {
            
        }

        public void WriteWord(ushort address, ushort value)
        {
            
        }
    }
}