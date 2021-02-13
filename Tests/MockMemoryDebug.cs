using System;
using System.Threading.Tasks;
using Debugger;
using HardwareCore;

namespace Tests
{
    public class MockMemoryDebug : IAddressMap
    {
        public EventHandler<MemoryChangedEventArgs> MemoryChanged { get; set; }

        public bool CanRead => throw new NotImplementedException();

        public bool CanWrite => throw new NotImplementedException();

        public ushort StartAddress => throw new NotImplementedException();

        public uint Size => throw new NotImplementedException();

        public ushort LowWaterMark => throw new NotImplementedException();

        public ushort HighWaterMark => throw new NotImplementedException();

        public ILoaderLabelTable Labels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Task Initialise()
        {
            throw new NotImplementedException();
        }

        public void Install(IAddressAssignment device)
        {
            throw new NotImplementedException();
        }

        public ILoader Load()
        {
            throw new NotImplementedException();
        }

        public ILoader Load(ushort startAddress)
        {
            throw new NotImplementedException();
        }

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

        public void ResetWatermarks()
        {
            throw new NotImplementedException();
        }

        public void Write(ushort address, byte value)
        {
            
        }

        public void WriteWord(ushort address, ushort value)
        {
            
        }
    }
}