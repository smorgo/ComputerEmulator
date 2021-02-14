using System;
using System.Threading.Tasks;
using Debugger;
using HardwareCore;

namespace Tests
{
    public class MockMemoryDebug : IAddressMap
    {
        private byte[] _memory;
        public EventHandler<MemoryChangedEventArgs> MemoryChanged { get; set; }

        public bool CanRead => throw new NotImplementedException();

        public bool CanWrite => throw new NotImplementedException();

        public ushort StartAddress => throw new NotImplementedException();

        public uint Size => throw new NotImplementedException();

        public ushort LowWaterMark => throw new NotImplementedException();

        public ushort HighWaterMark => throw new NotImplementedException();

        public ILoaderLabelTable Labels { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public MockMemoryDebug()
        {
            _memory = new byte[0x10000];
            AsyncUtil.RunSync(Initialise);
        }
        public async Task Initialise()
        {
            for(var ix = 0; ix < 0x10000; ix++)
            {
                _memory[ix] = (byte)(ix & 0xFF);
            }

            await Task.Delay(0);
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
            return _memory[address];
        }

        public byte[] ReadBlock(ushort startAddress, ushort endAddress)
        {
            var start = Math.Min(startAddress, endAddress);
            var end = Math.Max(startAddress, endAddress);
            var size = end - start + 1;
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
            _memory[address] = value;
        }

        public void WriteWord(ushort address, ushort value)
        {
            _memory[address] = (byte)(value & 0xff);
            _memory[address+1] = (byte)((value >> 8) & 0xff);
        }
    }
}