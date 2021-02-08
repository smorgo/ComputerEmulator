using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using HardwareCore;

namespace Memory
{
    public class Ram : IAddressAssignment, IAddressableBlock
    {
        public bool CanRead => true;
        public bool CanWrite => true;
        public ushort StartAddress {get; private set;}
        public UInt32 Size {get; private set;}

        public List<IAddressableBlock> Blocks => new List<IAddressableBlock> {this};

        public IAddressAssignment Device => this;

        public int BlockId => 0;

        private Byte[] Memory;

        public Ram(ushort absoluteAddress, UInt32 size) 
        {
            Debug.Assert(absoluteAddress + size <= 0x10000);
            StartAddress = absoluteAddress;
            Size = size;
            Memory = new byte[size];
        }

        public void Write(ushort address, byte value)
        {
            // address is now relative to start address
            Debug.Assert(address < Size);
            Memory[address] = value;
        }

        public byte Read(ushort address)
        {
            // address is now relative to start address
            Debug.Assert(address < Size);
            return Memory[address];
        }

        public async Task Initialise()
        {
            Array.Fill<byte>(Memory, 0x00);
            await Task.Delay(0);
        }

        public void Write(int blockId, ushort address, byte value)
        {
            Write(address, value);
        }

        public byte Read(int blockId, ushort address)
        {
            return Read(address);
        }
    }
}